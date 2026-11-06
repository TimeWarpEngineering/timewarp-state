#region Purpose
// Release half of `dev workflow`: tag-gate → check-version → locate-run → download-artifact → verify → push.
#endregion

#region Design
// Promotes the CI Packages-* artifact (Nuru 458 / Ganda 209); no rebuild. Returns a PipelineResult and never
// touches Environment.ExitCode itself (check-version still reports through it; PipelineConsole.RunStepAsync
// reads it). Any exception from a service, including cancellation, aborts with the step that was running.
// The version comes from AssertVersionSsot once and is passed to verify and push.
// A check-version that finishes non-zero aborts with CheckVersionAbort.FormatAbortReason (from the handler's
// Outcome), so only AlreadyReleased says "version already released"; cancel and throw name themselves.
// The packable-set lookup sets CurrentStep before it runs so its failures are not blamed on check-version.
// Locate-run may promote the artifact of a green commit HEAD is only kanban-only commits past (task 133);
// it then prints that commit, and the download and verify messages name it instead of HEAD. The tag pin
// still compares the tag with HEAD: a resume runs from the tag's ref, where the walk is empty.
#endregion

namespace DevCli.Commands;

internal sealed class ReleasePipeline
{
  private readonly PipelineConsole Output;
  private readonly ITerminal Terminal;
  private readonly CheckVersionCommand.Handler CheckVersionHandler;
  private readonly IPackableProjectService PackableProjectService;
  private string CurrentStep = "Release setup";

  public ReleasePipeline
  (
    PipelineConsole console,
    CheckVersionCommand.Handler checkVersionHandler,
    IPackableProjectService packableProjectService
  )
  {
    Output = console;
    Terminal = console.Terminal;
    CheckVersionHandler = checkVersionHandler;
    PackableProjectService = packableProjectService;
  }

  public async Task<PipelineResult> RunAsync(string repoRoot, string? apiKey, CancellationToken ct)
  {
    Terminal.WriteLine("Pipeline: tag-gate → check-version → locate-run → download-artifact → verify → push\n");

    try
    {
      return await RunStepsAsync(repoRoot, apiKey, ct);
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
      return Output.Abort($"{CurrentStep} cancelled");
    }
    catch (Exception exception)
    {
      Terminal.WriteErrorLine($"{CurrentStep} failed: {exception.Message}");
      return Output.Abort($"{CurrentStep} failed");
    }
  }

  private void BeginStep(int number, string name)
  {
    CurrentStep = name;
    Output.Banner($"Step {number}/6: {name}");
  }

  private async Task<PipelineResult> RunStepsAsync(string repoRoot, string? apiKey, CancellationToken ct)
  {
    string? version = new RepoVersionService(Terminal, repoRoot).AssertVersionSsot();
    if (version is null)
    {
      return Output.Abort("Assert Version SSOT failed");
    }

    BeginStep(1, "Release Gate — Tag Assertions");
    PipelineResult? gateFailure = await RunTagGateAsync(version, ct);
    if (gateFailure is not null)
    {
      return gateFailure;
    }

    BeginStep(2, "Check Version");
    StepOutcome checkVersionOutcome = await Output.RunStepAsync("Check Version", () => CheckVersionHandler.Handle(new CheckVersionCommand(), ct), ct);
    PipelineResult? checkVersionFailure = CheckVersionFailure(checkVersionOutcome);
    if (checkVersionFailure is not null)
    {
      return checkVersionFailure;
    }

    CurrentStep = "Packable Set";
    IReadOnlyList<PackableProject> packableProjects = await PackableProjectService.GetPackableProjectsAsync(repoRoot, ct);
    if (packableProjects.Count == 0)
    {
      Terminal.WriteErrorLine("Release gate failed: no packable projects found under source/.");
      return Output.Abort("no packable projects found");
    }

    Terminal.WriteLine($"Packable set ({packableProjects.Count}): {string.Join(", ", packableProjects.Select(project => project.PackageId))}");

    BeginStep(3, "Locate CI Run");
    LocateRunOutcome locateOutcome = await CiArtifactService.LocateCiRunAsync(ct);
    PipelineResult? locateFailure = LocateFailure(locateOutcome);
    if (locateFailure is not null)
    {
      return locateFailure;
    }

    if (locateOutcome.SkippedCommitCount > 0)
    {
      Terminal.WriteLine($"HEAD {ShortSha(locateOutcome.HeadSha)} is {locateOutcome.SkippedCommitCount} kanban-only commit(s) past {ShortSha(locateOutcome.TargetSha)}; promoting the artifact built at {locateOutcome.TargetSha}.");
    }

    Terminal.WriteLine($"Found {locateOutcome.CandidateRuns.Count} candidate CI run(s) for commit {ShortSha(locateOutcome.TargetSha)}.");

    BeginStep(4, "Download Artifact");
    DownloadArtifactOutcome downloadOutcome = await new CiArtifactService(Terminal)
      .DownloadPackagesArtifactAsync(repoRoot, locateOutcome.CandidateRuns, ct);
    if (downloadOutcome.Status == DownloadArtifactStatus.Exhausted)
    {
      return DownloadFailure(downloadOutcome, locateOutcome);
    }

    Terminal.WriteLine($"Downloaded '{downloadOutcome.ArtifactName}' from run {downloadOutcome.Run!.DatabaseId} ({downloadOutcome.Run.Event}).");

    BeginStep(5, "Verify Package Set");
    PipelineResult? verifyFailure = VerifyPackageSet(repoRoot, packableProjects, version, locateOutcome.TargetSha);
    if (verifyFailure is not null)
    {
      return verifyFailure;
    }

    BeginStep(6, "Push to NuGet");
    await new NuGetPushService(Terminal).PushPackagesAsync(repoRoot, packableProjects, version, apiKey, ct);

    Output.Banner("Pipeline SUCCEEDED - Packages published to NuGet.org");
    return PipelineResult.Success;
  }

  private async Task<PipelineResult?> RunTagGateAsync(string version, CancellationToken ct)
  {
    if (Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME") == "release")
    {
      TagAssertionResult tagResult = TagAssertion.Validate(Environment.GetEnvironmentVariable("GITHUB_REF_NAME"), version);
      if (!tagResult.IsValid)
      {
        Terminal.WriteErrorLine($"Release gate failed: {tagResult.Error}");
        return Output.Abort("release tag does not match source version");
      }

      Terminal.WriteLine($"Tag assertion passed: {tagResult.ExpectedTag}");
    }
    else
    {
      Terminal.WriteLine("Tag assertion skipped: GITHUB_EVENT_NAME is not 'release' (break-glass/local release has no triggering ref tag to assert against; the tag-pin check below still applies).");
    }

    TagPinOutcome tagPin = await ReleaseGitGate.CheckTagPinAsync(version, ct);
    string pinTag = $"v{version}";
    switch (tagPin.Status)
    {
      case TagPinStatus.NoTag:
        Terminal.WriteLine($"Tag pin: {pinTag} not yet tagged.");
        break;
      case TagPinStatus.Match:
        Terminal.WriteLine($"Tag pin passed: HEAD is at {pinTag}.");
        break;
      case TagPinStatus.Mismatch:
        Terminal.WriteErrorLine($"Release gate failed: tag {pinTag} already exists at commit {ShortSha(tagPin.TagCommit)}; this run is at {ShortSha(tagPin.HeadCommit)}. A partial-publish resume must run from the tag's commit (or bump the version if source changed).");
        return Output.Abort("tag pin mismatch");
      case TagPinStatus.GitError:
        Terminal.WriteErrorLine($"Release gate failed: tag pin check could not run — {tagPin.Detail}");
        return Output.Abort("tag pin check could not run");
      default:
        return Output.AbortUnhandled("tag pin", tagPin.Status);
    }

    AncestorCheckOutcome ancestor = await new ReleaseGitGate(Terminal).CheckHeadAncestorOfMasterAsync(ct);
    switch (ancestor.Status)
    {
      case AncestorCheckStatus.Ancestor:
        return null;
      case AncestorCheckStatus.NotAncestor:
        Terminal.WriteErrorLine("Release gate failed: current commit is not an ancestor of master. Releases must be cut from commits on master.");
        return Output.Abort("commit not on master");
      case AncestorCheckStatus.MasterUnresolvable:
        Terminal.WriteErrorLine("Release gate failed: cannot resolve origin/master or master — ensure the checkout has full history (fetch-depth: 0) and a master ref exists.");
        return Output.Abort("master ref unresolvable");
      case AncestorCheckStatus.GitError:
        Terminal.WriteErrorLine($"Release gate failed: ancestor check could not run — {ancestor.Detail}");
        return Output.Abort("ancestor check could not run");
      default:
        return Output.AbortUnhandled("ancestor check", ancestor.Status);
    }
  }

  private PipelineResult? CheckVersionFailure(StepOutcome outcome)
  {
    switch (outcome)
    {
      case StepOutcome.Succeeded:
        return null;
      case StepOutcome.Failed:
        return Output.Abort(CheckVersionAbort.FormatAbortReason(CheckVersionHandler.Outcome, CheckVersionHandler.GitHubPackagesFailure));
      case StepOutcome.Cancelled:
        return Output.Abort("Check Version cancelled");
      case StepOutcome.Threw:
        return Output.Abort("Check Version threw");
      default:
        return Output.AbortUnhandled("check-version step", outcome);
    }
  }

  private PipelineResult? LocateFailure(LocateRunOutcome outcome)
  {
    switch (outcome.Status)
    {
      case LocateRunStatus.Found:
        return null;
      case LocateRunStatus.GhUnavailable:
        Terminal.WriteErrorLine("Release gate failed: release mode promotes CI-built artifacts and requires the gh CLI. On runners GH_TOKEN is provided by workflow.yml; locally install gh and run 'gh auth login'.");
        return Output.Abort("gh CLI unavailable");
      case LocateRunStatus.GhFailed:
        Terminal.WriteErrorLine($"Release gate failed: gh run list failed — {outcome.Detail}. If this is transient (network/rate limit), retry; for auth issues run 'gh auth login'.");
        return Output.Abort("gh run list failed");
      case LocateRunStatus.NoMatchingRun:
        string walkDetail = outcome.Detail is null ? "" : $" {outcome.Detail}";
        Terminal.WriteErrorLine($"Release gate failed: no successful CI run of workflow.yml exists for commit {outcome.HeadSha}, or for a commit it is only kanban-only commits past.{walkDetail} Only tested CI artifacts are published — this commit must pass CI first. If a run failed, fix and re-run it (gh run rerun <run-id>).");
        return Output.Abort("no successful CI run found");
      default:
        return Output.AbortUnhandled("locate-run", outcome.Status);
    }
  }

  private PipelineResult DownloadFailure(DownloadArtifactOutcome download, LocateRunOutcome locate)
  {
    if (download.ExpiredEncounters.Count > 0)
    {
      string expiredDetail = string.Join("; ", download.ExpiredEncounters.Select(encounter => $"run {encounter.RunId} ({encounter.Event}): {string.Join(", ", encounter.ArtifactNames)}"));
      Terminal.WriteErrorLine($"Release gate failed: every candidate CI run's Packages-* artifact has expired — {expiredDetail}. Re-run CI to produce a fresh tested artifact (gh run rerun {download.ExpiredEncounters[0].RunId}).");
    }
    else
    {
      Terminal.WriteErrorLine($"Release gate failed: no candidate CI run for commit {locate.TargetSha} uploaded a Packages-* artifact. Re-run CI to produce one (gh run rerun {locate.CandidateRuns[0].DatabaseId}).");
    }

    return Output.Abort("no usable CI artifact found");
  }

  private PipelineResult? VerifyPackageSet(string repoRoot, IReadOnlyList<PackableProject> packableProjects, string version, string artifactSha)
  {
    string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
    string[] actualNupkgPaths = Directory.Exists(artifactsDir) ? Directory.GetFiles(artifactsDir, "*.nupkg") : [];
    IReadOnlyList<string> actualFileNames = [.. actualNupkgPaths.Select(Path.GetFileName).OfType<string>()];

    PackageSetVerification verification = CiRunPromotion.VerifyPackageSet(actualFileNames, packableProjects, version);
    if (verification.IsMatch)
    {
      Terminal.WriteLine("Package set verified.");
      return null;
    }

    if (verification.Missing.Count > 0)
    {
      Terminal.WriteErrorLine($"Release gate failed: downloaded artifact is missing package(s): {string.Join(", ", verification.Missing)}.");
    }

    if (verification.Unexpected.Count > 0)
    {
      Terminal.WriteErrorLine($"Release gate failed: downloaded artifact has unexpected package(s): {string.Join(", ", verification.Unexpected)}.");
    }

    Terminal.WriteErrorLine($"CI run likely predates the version bump — re-run CI on commit {artifactSha} and retry.");
    return Output.Abort("downloaded package set does not match derived packable set");
  }

  private static string ShortSha(string? sha) =>
    string.IsNullOrEmpty(sha) ? "(unknown)" : sha.Length > 7 ? sha[..7] : sha;
}
