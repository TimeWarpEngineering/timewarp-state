#region Purpose
// Mode-aware CI/CD pipeline for TimeWarp.State
#endregion
#region Design
// PR/merge: clean -> build -> test -> verify-samples.
// Release:  GitHub workflow.yml owns NuGet Trusted Publishing (OIDC probe/push).
// This command runs the local gate so CI and developers share one entry point.
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
      Terminal.WriteLine("Pipeline: clean -> build -> test -> verify-samples\n");
      Environment.ExitCode = 0;

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
      Terminal.WriteLine("Pipeline: clean -> build -> check-version\n");
      Terminal.WriteLine("NuGet push stays in .github/workflows/workflow.yml (OIDC Trusted Publishing).\n");
      Environment.ExitCode = 0;

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
  }
}
