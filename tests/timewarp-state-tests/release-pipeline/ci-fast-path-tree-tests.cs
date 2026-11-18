#region Purpose
// Pins the CI fast-path kanban-only rule (task 162): two trees that differ only under kanban/ strip to the same
// tree, and a one-byte change under source/ does not.
#endregion

#region Design
// Runs the real rule: pwsh dot-sources scripts/ci-fast-path-tree.ps1 (the file the workflow gate dot-sources)
// inside a throwaway git repository built in the temp folder. Trees come from `git write-tree`, so no commit,
// author, or signing config is needed. GIT_DIR and GIT_WORK_TREE point at the temp repository so a git hook
// environment around the test run cannot redirect git. Processes go through Amuru (ProcessStartInfo is banned).
#endregion

namespace DevCli.Commands.CiFastPathTreeTests;

public class Should_
{
  public static async Task Strip_Trees_That_Differ_Only_Under_Kanban_To_The_Same_Tree()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string before = await repository.WriteTreeAsync();
    await repository.WriteFileAsync("kanban/to-do/001-task/task.md", "# Task\n\nmoved and edited\n");
    await repository.WriteFileAsync("kanban/done/002-new/task.md", "# New\n");
    string after = await repository.WriteTreeAsync();

    after.ShouldNotBe(before);
    (await repository.StripAsync(after)).ShouldBe(await repository.StripAsync(before));
    (await repository.DifferenceAsync(before, after)).ShouldBeEmpty();
  }

  public static async Task Keep_Trees_Apart_When_One_Byte_Under_Source_Differs()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string before = await repository.WriteTreeAsync();
    await repository.WriteFileAsync("source/lib/a.cs", "class A { }\n ");
    string after = await repository.WriteTreeAsync();

    (await repository.StripAsync(after)).ShouldNotBe(await repository.StripAsync(before));
    (await repository.DifferenceAsync(before, after)).ShouldBe("differs outside kanban/: source/lib/a.cs");
  }

  // why: task 162 review round 1 M1, rename detection listed only kanban/a.cs and the reason named no path
  public static async Task Name_The_Source_Path_When_A_File_Moves_Into_Kanban()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string before = await repository.WriteTreeAsync();
    await repository.MoveFileAsync("source/lib/a.cs", "kanban/a.cs");
    string after = await repository.WriteTreeAsync();

    (await repository.DifferenceAsync(before, after)).ShouldBe("differs outside kanban/: source/lib/a.cs");
  }

  public static async Task Keep_Trees_Apart_When_A_Nested_Kanban_Folder_Differs()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string before = await repository.WriteTreeAsync();
    await repository.WriteFileAsync("source/kanban/note.md", "not the board\n");
    string after = await repository.WriteTreeAsync();

    (await repository.StripAsync(after)).ShouldNotBe(await repository.StripAsync(before));
  }

  private sealed class TempRepository : IDisposable
  {
    private readonly string root;
    private readonly string helper;

    private TempRepository(string root, string helper)
    {
      this.root = root;
      this.helper = helper;
    }

    public static async Task<TempRepository> CreateAsync()
    {
      string root = Directory.CreateTempSubdirectory("ci-fast-path-tree-").FullName;
      string helper = Path.Combine(FindRepoRoot(), "scripts", "ci-fast-path-tree.ps1");
      TempRepository repository = new(root, helper);
      await repository.RunAsync("git", "init", "--quiet");
      await repository.WriteFileAsync("source/lib/a.cs", "class A { }\n");
      await repository.WriteFileAsync("readme.md", "# Repo\n");
      await repository.WriteFileAsync("kanban/to-do/001-task/task.md", "# Task\n");
      return repository;
    }

    public async Task WriteFileAsync(string relativePath, string content)
    {
      string path = Path.Combine(root, relativePath);
      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      await File.WriteAllTextAsync(path, content);
    }

    public Task MoveFileAsync(string fromPath, string toPath)
    {
      string target = Path.Combine(root, toPath);
      Directory.CreateDirectory(Path.GetDirectoryName(target)!);
      File.Move(Path.Combine(root, fromPath), target);
      return Task.CompletedTask;
    }

    public async Task<string> WriteTreeAsync()
    {
      await RunAsync("git", "add", "--all");
      return await RunAsync("git", "write-tree");
    }

    public Task<string> StripAsync(string tree) =>
      RunAsync("pwsh", "-NoProfile", "-NonInteractive", "-Command", $". '{helper}'; Get-TreeWithoutKanban '{tree}'");

    public Task<string> DifferenceAsync(string tree, string otherTree) =>
      RunAsync("pwsh", "-NoProfile", "-NonInteractive", "-Command", $". '{helper}'; Get-KanbanOnlyDifference '{tree}' '{otherTree}'");

    private async Task<string> RunAsync(string command, params string[] arguments)
    {
      CommandOutput output = await Shell.Builder(command)
        .WithArguments(arguments)
        .WithWorkingDirectory(root)
        .WithEnvironmentVariable("GIT_DIR", Path.Combine(root, ".git"))
        .WithEnvironmentVariable("GIT_WORK_TREE", root)
        .WithNoValidation()
        .CaptureAsync();
      output.ExitCode.ShouldBe(0, $"{command} {string.Join(' ', arguments)} failed: {output.Stderr}");
      return output.Stdout.Trim();
    }

    public void Dispose() => Directory.Delete(root, recursive: true);
  }

  private static string FindRepoRoot()
  {
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "timewarp-state.slnx")))
    {
      directory = directory.Parent;
    }

    return directory?.FullName ?? throw new InvalidOperationException("timewarp-state.slnx not found above the test output.");
  }
}
