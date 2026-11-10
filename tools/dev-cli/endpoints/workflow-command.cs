#region Purpose
// Mode-aware CI/CD pipeline for TimeWarp.State
#endregion
#region Design
// PR/merge: assert-version-ssot → clean → build → test → e2e → verify-samples.
// verify-samples packs, then restores sample 00 auto from artifacts/packages.
// The other samples ProjectReference source and are part of the build filter.
// Release: ReleasePipeline (workflow/release-pipeline.cs) promotes the CI Packages-* artifact — no rebuild.
// TimeWarpStateVersion (CPM / samples) must equal source/ Directory.Build.props
// Version (pack / check-version); RepoVersionService.AssertVersionSsot fails on drift.
// Handlers are invoked directly so the pipeline does not need a pre-installed ./bin/dev.
// Both pipelines return a PipelineResult; this handler is the only place that sets the final
// Environment.ExitCode. The NuGet API key comes from --api-key or NUGET_API_KEY (CI uses the env var
// so the key stays off the dev argv; see workflow/nuget-push-service.cs for the remaining exposure).
#endregion

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Execute full CI/CD pipeline (mode-aware)")]
internal sealed class WorkflowCommand : ICommand<Unit>
{
  internal const string ApiKeyEnvironmentVariable = "NUGET_API_KEY";

  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME)")]
  public string? Mode { get; set; }

  [Option("api-key", "k", Description = "NuGet API key for publishing (prefer the NUGET_API_KEY env var; argv is visible in the process list)")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : IRequestHandler<WorkflowCommand, Unit>
  {
    private readonly PipelineConsole Output;
    private readonly ITerminal Terminal;
    private readonly IRepoCleanService RepoCleanService;
    private readonly NuGetVersionService NuGetVersionService;
    private readonly IRepoConfigService RepoConfigService;
    private readonly IPackableProjectService PackableProjectService;

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
      Output = new PipelineConsole(terminal);
      RepoCleanService = repoCleanService;
      NuGetVersionService = nuGetVersionService;
      RepoConfigService = repoConfigService;
      PackableProjectService = packableProjectService;
    }

    public async ValueTask<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      CiMode mode = CiModeDetector.DetermineMode(command.Mode, eventName);
      if (string.IsNullOrEmpty(command.Mode))
      {
        Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {eventName ?? "(not set)"} -> Mode: {mode}");
      }

      Terminal.WriteLine($"\nCI/CD Pipeline — Mode: {mode}\n".Cyan());

      PipelineResult result = await RunAsync(mode, command, ct);
      Environment.ExitCode = result.Succeeded ? 0 : 1;
      return Unit.Value;
    }

    private async Task<PipelineResult> RunAsync(CiMode mode, WorkflowCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        return Output.Abort("repository root not found");
      }

      if (mode != CiMode.Release)
      {
        return await RunPrAsync(repoRoot, ct);
      }

      string? apiKey = string.IsNullOrEmpty(command.ApiKey)
        ? Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable)
        : command.ApiKey;
      CheckVersionCommand.Handler checkVersionHandler = new(Terminal, NuGetVersionService, RepoConfigService, PackableProjectService);
      return await new ReleasePipeline(Output, checkVersionHandler, PackableProjectService).RunAsync(repoRoot, apiKey, ct);
    }

    private async Task<PipelineResult> RunPrAsync(string repoRoot, CancellationToken ct)
    {
      Terminal.WriteLine("Pipeline: assert-version-ssot → clean → build → test → e2e → verify-samples\n");

      if (new RepoVersionService(Terminal, repoRoot).AssertVersionSsot() is null)
      {
        Terminal.WriteErrorLine("\nPipeline FAILED — Assert Version SSOT failed".Red());
        return PipelineResult.Failed("Assert Version SSOT failed");
      }

      (string Name, Func<Task> Run)[] steps =
      [
        ("Clean", () => new CleanCommand.Handler(Terminal, RepoCleanService).Handle(new CleanCommand(), ct).AsTask()),
        ("Build", () => new BuildCommand.Handler(Terminal).Handle(new BuildCommand(), ct).AsTask()),
        ("Test", () => new TestCommand.Handler(Terminal).Handle(new TestCommand(), ct).AsTask()),
        ("E2E", () => new E2eCommand.Handler(Terminal).Handle(new E2eCommand(), ct).AsTask()),
        ("Verify Samples", () => new VerifySamplesCommand.Handler(Terminal, PackableProjectService).Handle(new VerifySamplesCommand(), ct).AsTask())
      ];

      foreach ((string name, Func<Task> run) in steps)
      {
        if (await Output.RunStepAsync(name, run, ct) != StepOutcome.Succeeded)
        {
          return PipelineResult.Failed($"{name} failed");
        }
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
      return PipelineResult.Success;
    }
  }
}
