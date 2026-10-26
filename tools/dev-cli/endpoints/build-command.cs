#region Purpose
// Build TimeWarp.State source and tests in Release
#endregion
#region Design
// Derives a solution filter from timewarp-state.slnx that omits samples/.
// Samples PackageReference TimeWarp.State / Plus at TimeWarpStateVersion.
// That version is not on nuget.org. verify-samples restores them with
// --source artifacts/packages after pack. Root nuget.config does not list that
// folder, so a missing feed is not NU1301 on this build. Filter JSON is written
// by hand because PublishAot disables reflection JsonSerializer. Restores land in
// artifacts/nuget-packages (dev.cs sets NUGET_PACKAGES; RestorePackagesPath agrees).
#endregion

namespace DevCli.Commands;

[NuruRoute("build", Description = "Build source and test projects (samples restore after pack)")]
internal sealed class BuildCommand : ICommand<Unit>
{
  [Option("clean", "c", Description = "Clean before building")]
  public bool Clean { get; set; }

  [Option("quiet", "q", Description = "Hide build output unless the command fails")]
  public bool Quiet { get; set; }

  internal sealed class Handler : ICommandHandler<BuildCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private BuildCommand Command = null!;
    private CancellationToken Ct;
    private string RepoRoot = null!;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(BuildCommand command, CancellationToken ct)
    {
      Command = command;
      Ct = ct;

      if (!FindRepoRoot())
      {
        return Value;
      }

      if (!await CleanAsync())
      {
        return Value;
      }

      if (!await BuildAsync())
      {
        return Value;
      }

      Terminal.WriteLine("\nBuild completed successfully!".Green());
      return Value;
    }

    private bool FindRepoRoot()
    {
      string? root = Git.FindRoot();
      if (root is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return false;
      }

      RepoRoot = root;
      Terminal.WriteLine($"Building repository at {RepoRoot}...");
      return true;
    }

    private async Task<bool> CleanAsync()
    {
      if (!Command.Clean)
      {
        return true;
      }

      string solutionFile = Path.Combine(RepoRoot, "timewarp-state.slnx");
      Terminal.WriteLine($"\nCleaning {solutionFile}...");
      CommandResult command = DotNet.Clean()
        .WithProject(solutionFile)
        .WithNoValidation()
        .Build();

      return await ExecuteAsync(command, "Clean failed!");
    }

    private async Task<bool> BuildAsync()
    {
      string solutionFile = Path.Combine(RepoRoot, "timewarp-state.slnx");
      string? filterPath = TryWriteBuildFilter(solutionFile);
      if (filterPath is null)
      {
        return false;
      }

      Terminal.WriteLine($"\nBuilding {filterPath} (samples omitted until pack)...");
      CommandResult command = DotNet.Build()
        .WithProject(filterPath)
        .WithConfiguration("Release")
        .WithNoValidation()
        .Build();

      return await ExecuteAsync(command, "Build failed!");
    }

    private string? TryWriteBuildFilter(string solutionFile)
    {
      if (!File.Exists(solutionFile))
      {
        Terminal.WriteErrorLine($"Error: solution not found: {solutionFile}");
        Environment.ExitCode = 1;
        return null;
      }

      XDocument document = XDocument.Load(solutionFile);
      List<string> projectPaths = [];
      foreach (XElement projectElement in document.Descendants("Project"))
      {
        string? path = projectElement.Attribute("Path")?.Value;
        if (string.IsNullOrWhiteSpace(path))
        {
          continue;
        }

        string normalized = path.Replace('\\', '/');
        if (normalized.StartsWith("samples/", StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        projectPaths.Add(path.Replace('/', '\\'));
      }

      projectPaths =
      [
        .. projectPaths
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
      ];

      if (projectPaths.Count == 0)
      {
        Terminal.WriteErrorLine("Error: no non-sample projects found in timewarp-state.slnx.");
        Environment.ExitCode = 1;
        return null;
      }

      Directory.CreateDirectory(Path.Combine(RepoRoot, "artifacts"));
      string filterPath = Path.Combine(RepoRoot, "artifacts", "timewarp-state.build.slnf");
      string projectsJson = string.Join(",\n      ", projectPaths.Select(QuoteJsonString));
      string json =
        "{\n" +
        "  \"solution\": {\n" +
        $"    \"path\": {QuoteJsonString(solutionFile)},\n" +
        "    \"projects\": [\n" +
        $"      {projectsJson}\n" +
        "    ]\n" +
        "  }\n" +
        "}\n";
      File.WriteAllText(filterPath, json);
      Terminal.WriteLine($"Build filter: {projectPaths.Count} project(s); samples omitted.");
      return filterPath;
    }

    private static string QuoteJsonString(string value) =>
      $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private async Task<bool> ExecuteAsync(CommandResult command, string failureMessage)
    {
      if (Command.Quiet)
      {
        CommandOutput result = await command.CaptureAsync(Ct);
        if (!result.Success)
        {
          Terminal.WriteErrorLine(result.Combined);
          Terminal.WriteErrorLine(failureMessage.Red());
          Environment.ExitCode = 1;
          return false;
        }

        return true;
      }

      int exitCode = await command.RunAsync(Ct);
      if (exitCode != 0)
      {
        Terminal.WriteErrorLine(failureMessage.Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }
  }
}
