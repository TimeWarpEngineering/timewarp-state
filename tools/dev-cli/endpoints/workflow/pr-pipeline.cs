#region Purpose
// The PR/merge pipeline stages: assert-version-ssot → clean → build → test → e2e → verify-samples.
#endregion

#region Design
// One stage list for `dev workflow` (pull request and merge, after the build classifier) and `dev ci` (the
// local run that writes the CI note). Keeping it in one place means a note can only claim the stages CI
// would have run. The stage ids the note records are CiNoteInput.StageIds; clean is housekeeping, not a stage.
// Handlers are invoked directly so the pipeline does not need a pre-installed ./bin/dev.
// why: task 150, `dev ci` shares the stages with `dev workflow`
#endregion

namespace DevCli.Commands;

internal sealed class PrPipeline
{
  private readonly PipelineConsole Output;
  private readonly IRepoCleanService RepoCleanService;
  private readonly IPackableProjectService PackableProjectService;

  public PrPipeline(PipelineConsole output, IRepoCleanService repoCleanService, IPackableProjectService packableProjectService)
  {
    Output = output;
    RepoCleanService = repoCleanService;
    PackableProjectService = packableProjectService;
  }

  private ITerminal Terminal => Output.Terminal;

  public async Task<PipelineResult> RunAsync(string repoRoot, CancellationToken ct)
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
