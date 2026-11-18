#region Purpose
// Pins the CI fast-path kanban-only rule (task 162): two trees that differ only under kanban/ strip to the same
// tree, and a one-byte change under source/ does not. Pins the ancestor walk (task 163): a note two kanban-only
// commits behind the candidate hits, a commit that differs outside kanban/ stops the chain, and the walk stops
// after ten ancestors.
#endregion

#region Design
// Runs the real rule: pwsh dot-sources scripts/ci-fast-path-tree.ps1 (the file the workflow gate dot-sources)
// inside a throwaway git repository built in the temp folder. Trees come from `git write-tree`; the walk tests
// build commits with `git commit-tree --no-gpg-sign` and fixed author and committer variables, and put notes on
// trees the way `dev ci` does. GIT_CONFIG_GLOBAL and GIT_CONFIG_NOSYSTEM keep the user's git config (signing,
// hooks) out. GIT_DIR and GIT_WORK_TREE point at the temp repository so a git hook environment around the test
// run cannot redirect git. Processes go through Amuru (ProcessStartInfo is banned).
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

  // why: task 163, PR #694 run 44330457502 missed the attested commit one land-pr commit behind the PR head
  public static async Task Hit_The_Attested_Commit_Two_Kanban_Only_Commits_Behind_The_Candidate()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string attested = await repository.CommitAsync(null);
    await repository.NoteAsync(attested);
    await repository.WriteFileAsync("kanban/to-do/001-task/task.md", "# Task\n\nlanded\n");
    string landed = await repository.CommitAsync(attested);
    await repository.WriteFileAsync("kanban/done/001-task/task.md", "# Task\n");
    string head = await repository.CommitAsync(landed);
    await repository.WriteFileAsync("kanban/done/002-merge/task.md", "# Master\n");
    string mergeTree = await repository.WriteTreeAsync();

    string result = await repository.FindAsync(mergeTree, ("pr-head", head));

    result.ShouldBe($"hit pr-head ancestor 2 {attested} {await repository.TreeOfAsync(attested)}");
  }

  public static async Task Stop_The_Chain_At_A_Commit_That_Differs_Outside_Kanban()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    await repository.WriteFileAsync("source/lib/a.cs", "class A { int b; }\n");
    await repository.WriteFileAsync("kanban/to-do/001-task/task.md", "# Task\n\nolder\n");
    string older = await repository.CommitAsync(null);
    await repository.NoteAsync(older);
    await repository.WriteFileAsync("source/lib/a.cs", "class A { }\n");
    await repository.WriteFileAsync("kanban/to-do/001-task/task.md", "# Task\n");
    string attested = await repository.CommitAsync(older);
    await repository.NoteAsync(attested);
    await repository.WriteFileAsync("source/lib/a.cs", "class A { int b; }\n");
    string source = await repository.CommitAsync(attested);
    await repository.WriteFileAsync("kanban/done/001-task/task.md", "# Task\n");
    string head = await repository.CommitAsync(source);

    string result = await repository.FindAsync(await repository.TreeOfAsync(head), ("pr-head", head));

    result.ShouldStartWith("miss ");
    result.ShouldContain(
      $"pr-head ancestor 2 {attested}: attested tree {await repository.TreeOfAsync(attested)} differs outside kanban/: source/lib/a.cs");
    // older's tree equals HEAD's outside kanban/ and has a note: reaching it would have been a hit.
    result.ShouldNotContain(older);
  }

  public static async Task Stop_The_Chain_After_Ten_Ancestors()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string attested = await repository.CommitAsync(null);
    await repository.NoteAsync(attested);
    string commit = attested;
    string tenth = attested;
    for (int index = 1; index <= 11; index++)
    {
      await repository.WriteFileAsync($"kanban/done/{index:000}-task/task.md", "# Task\n");
      commit = await repository.CommitAsync(commit);
      if (index == 10)
      {
        tenth = commit;
      }
    }

    string tree = await repository.TreeOfAsync(commit);
    (await repository.FindAsync(tree, ("pr-head", tenth))).ShouldStartWith($"hit pr-head ancestor 10 {attested} ");
    (await repository.FindAsync(tree, ("pr-head", commit)))
      .ShouldBe($"miss pr-head {commit}: no CI note for tree {tree}; pr-head: no CI note within 10 kanban-only ancestors");
  }

  public static async Task Walk_A_Commit_Reached_From_Two_Candidates_Once()
  {
    using TempRepository repository = await TempRepository.CreateAsync();
    string root = await repository.CommitAsync(null);
    await repository.WriteFileAsync("kanban/done/001-task/task.md", "# Task\n");
    string head = await repository.CommitAsync(root);

    string result = await repository.FindAsync(await repository.TreeOfAsync(head), ("pr-head", head), ("parent 2", head));

    result.ShouldBe(
      $"miss pr-head {head}: no CI note for tree {await repository.TreeOfAsync(head)}; pr-head: no CI note down to the root commit at ancestor 1");
  }

  private sealed class TempRepository : IDisposable
  {
    private const string NotesRef = "refs/notes/ganda-ci";
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

    public async Task<string> CommitAsync(string? parent)
    {
      string tree = await WriteTreeAsync();
      return parent is null
        ? await RunAsync("git", "commit-tree", "--no-gpg-sign", "-m", "commit", tree)
        : await RunAsync("git", "commit-tree", "--no-gpg-sign", "-p", parent, "-m", "commit", tree);
    }

    public Task<string> TreeOfAsync(string commit) => RunAsync("git", "rev-parse", $"{commit}^{{tree}}");

    public async Task NoteAsync(string commit)
    {
      string tree = await TreeOfAsync(commit);
      await RunAsync("git", "notes", $"--ref={NotesRef}", "add", "-m", $"{{\"tree\":\"{tree}\"}}", tree);
    }

    public Task<string> FindAsync(string headTree, params (string Label, string Commit)[] candidates)
    {
      string list = string.Join(", ", candidates.Select(candidate => $"@{{ Label = '{candidate.Label}'; Commit = '{candidate.Commit}' }}"));
      return RunAsync(
        "pwsh", "-NoProfile", "-NonInteractive", "-Command",
        $". '{helper}'; $found = Find-AttestedTree -HeadTree '{headTree}' -Candidates @({list}) -NotesRef '{NotesRef}'; " +
        "if ($found.Tree) { \"hit $($found.Label) $($found.Tree)\" } else { \"miss $($found.Reasons -join '; ')\" }");
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
        .WithEnvironmentVariable("GIT_CONFIG_GLOBAL", "/dev/null")
        .WithEnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1")
        .WithEnvironmentVariable("GIT_AUTHOR_NAME", "Test")
        .WithEnvironmentVariable("GIT_AUTHOR_EMAIL", "test@example.invalid")
        .WithEnvironmentVariable("GIT_COMMITTER_NAME", "Test")
        .WithEnvironmentVariable("GIT_COMMITTER_EMAIL", "test@example.invalid")
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
