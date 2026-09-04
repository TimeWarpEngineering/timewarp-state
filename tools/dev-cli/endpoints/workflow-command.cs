#region Purpose
// Mode-aware CI/CD pipeline for TimeWarp.State
#endregion
#region Design
// PR/merge: assert-version-ssot -> clean -> build -> test -> verify-samples.
// Release:  assert-version-ssot -> clean -> build -> check-version.
// GitHub workflow.yml owns NuGet Trusted Publishing (OIDC probe/push).
// TimeWarpStateVersion (CPM / samples) must equal source/ Directory.Build.props
// Version (pack / check-version); the audit requires a literal source Version so
// the two cannot be one MSBuild property. AssertVersionSsot fails on drift.
// Handlers are invoked directly so the pipeline does not need a pre-installed ./bin/dev.
#endregion

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Execute full CI/CD pipeline (mode-aware)")]
internal sealed class WorkflowCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME)")]
  public string? Mode { get; set; }

  [Option("api-key", "k", Description = "NuGet API key for publishing (from OIDC Trusted Publishing)")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : ICommandHandler<WorkflowCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly IRepoCleanService RepoCleanService;
    private readonly NuGetVersionService NuGetVersionService;
    private readonly IRepoConfigService RepoConfigService;
    private readonly IPackableProjectService PackableProjectService;
    private CancellationToken Ct;

    public Handler
    (
      ITerminal terminal,
      IRepoCleanService repoCleanService,
      NuGetVersionService nuGetVersionService,
      IRepoConfigService repoConfigService,
      IPackableProjectService packableProjectService
    )
    {
      Terminal = terminal;
      RepoCleanService = repoCleanService;
      NuGetVersionService = nuGetVersionService;
      RepoConfigService = repoConfigService;
      PackableProjectService = packableProjectService;
    }

    public async ValueTask<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      Ct = ct;

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      CiMode mode = CiModeDetector.DetermineMode(command.Mode, eventName);
      if (string.IsNullOrEmpty(command.Mode))
      {
        string displayEventName = eventName ?? "(not set)";
        Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {displayEventName} -> Mode: {mode}");
      }

      Terminal.WriteLine($"\nCI/CD Pipeline — Mode: {mode}\n".Cyan());

      if (mode == CiMode.Release)
      {
        await RunReleaseAsync();
      }
      else
      {
        await RunPrAsync();
      }

      return Value;
    }

    private async Task RunPrAsync()
    {
      Terminal.WriteLine("Pipeline: assert-version-ssot -> clean -> build -> test -> verify-samples\n");
      Environment.ExitCode = 0;

      if (!AssertVersionSsot())
      {
        Terminal.WriteErrorLine("\nPipeline FAILED — Assert Version SSOT failed".Red());
        return;
      }

      if (!await RunStepAsync("Clean", new CleanCommand.Handler(Terminal, RepoCleanService).Handle(new CleanCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Build", new BuildCommand.Handler(Terminal).Handle(new BuildCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Test", new TestCommand.Handler(Terminal).Handle(new TestCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Verify Samples", new VerifySamplesCommand.Handler(Terminal).Handle(new VerifySamplesCommand(), Ct)))
      {
        return;
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
    }

    private async Task RunReleaseAsync()
    {
      Terminal.WriteLine("Pipeline: assert-version-ssot -> clean -> build -> check-version\n");
      Terminal.WriteLine("NuGet push stays in .github/workflows/workflow.yml (OIDC Trusted Publishing).\n");
      Environment.ExitCode = 0;

      if (!AssertVersionSsot())
      {
        Terminal.WriteErrorLine("\nPipeline FAILED — Assert Version SSOT failed".Red());
        return;
      }

      if (!await RunStepAsync("Clean", new CleanCommand.Handler(Terminal, RepoCleanService).Handle(new CleanCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Build", new BuildCommand.Handler(Terminal).Handle(new BuildCommand(), Ct)))
      {
        return;
      }

      CheckVersionCommand.Handler checkVersionHandler = new(
        Terminal,
        NuGetVersionService,
        RepoConfigService,
        PackableProjectService);
      if (!await RunStepAsync("Check Version", checkVersionHandler.Handle(new CheckVersionCommand(), Ct)))
      {
        return;
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
    }

    private async Task<bool> RunStepAsync(string stepName, ValueTask<Unit> step)
    {
      await step;

      if (Environment.ExitCode != 0)
      {
        Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} failed".Red());
        return false;
      }

      return true;
    }

    private bool AssertVersionSsot()
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return false;
      }

      string sourcePropsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      string repositoryPropsPath = Path.Combine(repoRoot, "msbuild", "repository.props");
      string? packVersion = TryReadMsBuildProperty(sourcePropsPath, "Version");
      string? cpmVersion = TryReadMsBuildProperty(repositoryPropsPath, "TimeWarpStateVersion");

      if (packVersion is not null
          && cpmVersion is not null
          && string.Equals(packVersion, cpmVersion, StringComparison.Ordinal))
      {
        Terminal.WriteLine($"Version SSOT aligned: {packVersion} (source Version == TimeWarpStateVersion)");
        return true;
      }

      string packDisplay = packVersion ?? "(missing)";
      string cpmDisplay = cpmVersion ?? "(missing)";
      Terminal.WriteErrorLine(
        $"Version SSOT mismatch: source/Directory.Build.props Version='{packDisplay}', msbuild/repository.props TimeWarpStateVersion='{cpmDisplay}'. Align both.".Red());
      Environment.ExitCode = 1;
      return false;
    }

    private static string? TryReadMsBuildProperty(string propsPath, string propertyName)
    {
      if (!File.Exists(propsPath))
      {
        return null;
      }

      XDocument document = XDocument.Load(propsPath);
      XElement? element = document
        .Descendants()
        .FirstOrDefault(candidate => string.Equals(candidate.Name.LocalName, propertyName, StringComparison.Ordinal));
      string? value = element?.Value?.Trim();
      return string.IsNullOrWhiteSpace(value) ? null : value;
    }
  }
}
