#region Purpose
// Mode-aware CI/CD pipeline for TimeWarp.State
#endregion
#region Design
// PR/merge: assert-version-ssot → clean → build → test → e2e → verify-samples.
// verify-samples packs, then restores sample 00 auto from artifacts/packages.
// The other samples ProjectReference source and are part of the build filter.
// Release: tag-gate → check-version → locate-run → download-artifact → verify → push.
// Release promotes the CI Packages-* artifact (Nuru 458 / Ganda 209) — no rebuild.
// TimeWarpStateVersion (CPM / samples) must equal source/ Directory.Build.props
// Version (pack / check-version); AssertVersionSsot fails on drift.
// Handlers are invoked directly so the pipeline does not need a pre-installed ./bin/dev.
#endregion

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Execute full CI/CD pipeline (mode-aware)")]
internal sealed partial class WorkflowCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME)")]
  public string? Mode { get; set; }

  [Option("api-key", "k", Description = "NuGet API key for publishing (from OIDC Trusted Publishing)")]
  public string? ApiKey { get; set; }

  internal sealed partial class Handler : ICommandHandler<WorkflowCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly IRepoCleanService RepoCleanService;
    private readonly NuGetVersionService NuGetVersionService;
    private readonly IRepoConfigService RepoConfigService;
    private readonly IPackableProjectService PackableProjectService;
    private CancellationToken Ct;
    private string? ApiKey;

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

    public async Task<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      Ct = ct;
      ApiKey = command.ApiKey;

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

      return Unit.Value;
    }

    private async Task RunPrAsync()
    {
      Terminal.WriteLine("Pipeline: assert-version-ssot → clean → build → test → e2e → verify-samples\n");
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

      if (!await RunStepAsync("E2E", new E2eCommand.Handler(Terminal).Handle(new E2eCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync(
        "Verify Samples",
        new VerifySamplesCommand.Handler(Terminal, PackableProjectService).Handle(new VerifySamplesCommand(), Ct)))
      {
        return;
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
    }

    private async Task RunReleaseAsync()
    {
      Terminal.WriteLine("Pipeline: tag-gate → check-version → locate-run → download-artifact → verify → push\n");
      Environment.ExitCode = 0;

      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        AbortPipeline("repository root not found");
        return;
      }

      if (!AssertVersionSsot())
      {
        AbortPipeline("Assert Version SSOT failed");
        return;
      }

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 1/6: Release Gate — Tag Assertions");
      Terminal.WriteLine("===============================================================================");

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      string? propsVersion = ReadPropsVersion(repoRoot);

      if (eventName == "release")
      {
        string? refName = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
        TagAssertionResult tagResult = TagAssertion.Validate(refName, propsVersion);

        if (!tagResult.IsValid)
        {
          Terminal.WriteErrorLine($"Release gate failed: {tagResult.Error}");
          AbortPipeline("release tag does not match source version");
          return;
        }

        Terminal.WriteLine($"Tag assertion passed: {tagResult.ExpectedTag}");
      }
      else
      {
        Terminal.WriteLine("Tag assertion skipped: GITHUB_EVENT_NAME is not 'release' (break-glass/local release has no triggering ref tag to assert against; the tag-pin check below still applies).");
      }

      if (string.IsNullOrWhiteSpace(propsVersion))
      {
        Terminal.WriteLine("Tag pin skipped: could not read <Version> from source/Directory.Build.props (Step 2/6 Check Version will fail with details).");
      }
      else
      {
        TagPinOutcome tagPinOutcome = await CheckTagPinAsync(propsVersion);
        string pinTag = $"v{propsVersion}";

        switch (tagPinOutcome.Status)
        {
          case TagPinStatus.NoTag:
            Terminal.WriteLine($"Tag pin: {pinTag} not yet tagged.");
            break;

          case TagPinStatus.Match:
            Terminal.WriteLine($"Tag pin passed: HEAD is at {pinTag}.");
            break;

          case TagPinStatus.Mismatch:
            Terminal.WriteErrorLine($"Release gate failed: tag {pinTag} already exists at commit {ShortSha(tagPinOutcome.TagCommit)}; this run is at {ShortSha(tagPinOutcome.HeadCommit)}. A partial-publish resume must run from the tag's commit (or bump the version if source changed).");
            AbortPipeline("tag pin mismatch");
            return;

          case TagPinStatus.GitError:
            Terminal.WriteErrorLine($"Release gate failed: tag pin check could not run — {tagPinOutcome.Detail}");
            AbortPipeline("tag pin check could not run");
            return;

          default:
            Terminal.WriteErrorLine($"Release gate failed: unhandled tag pin status '{tagPinOutcome.Status}'.");
            AbortPipeline("unhandled tag pin status");
            return;
        }
      }

      AncestorCheckOutcome ancestorOutcome = await CheckHeadAncestorOfMasterAsync();

      switch (ancestorOutcome.Status)
      {
        case AncestorCheckStatus.NotAncestor:
          Terminal.WriteErrorLine("Release gate failed: current commit is not an ancestor of master. Releases must be cut from commits on master.");
          AbortPipeline("commit not on master");
          return;

        case AncestorCheckStatus.MasterUnresolvable:
          Terminal.WriteErrorLine("Release gate failed: cannot resolve origin/master or master — ensure the checkout has full history (fetch-depth: 0) and a master ref exists.");
          AbortPipeline("master ref unresolvable");
          return;

        case AncestorCheckStatus.GitError:
          Terminal.WriteErrorLine($"Release gate failed: ancestor check could not run — {ancestorOutcome.Detail}");
          AbortPipeline("ancestor check could not run");
          return;

        case AncestorCheckStatus.Ancestor:
          break;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled ancestor check status '{ancestorOutcome.Status}'.");
          AbortPipeline("unhandled ancestor check status");
          return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 2/6: Check Version");
      Terminal.WriteLine("===============================================================================");
      CheckVersionCommand.Handler checkVersionHandler = new(
        Terminal,
        NuGetVersionService,
        RepoConfigService,
        PackableProjectService);
      if (!await RunStepAsync("Check Version", checkVersionHandler.Handle(new CheckVersionCommand(), Ct)))
      {
        AbortPipeline("version already released");
        return;
      }

      IReadOnlyList<PackableProject> packableProjects = await PackableProjectService
        .GetPackableProjectsAsync(repoRoot, Ct)
        .ConfigureAwait(false);

      if (packableProjects.Count == 0)
      {
        Terminal.WriteErrorLine("Release gate failed: no packable projects found under source/.");
        AbortPipeline("no packable projects found");
        return;
      }

      Terminal.WriteLine($"Packable set ({packableProjects.Count}): {string.Join(", ", packableProjects.Select(project => project.PackageId))}");

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 3/6: Locate CI Run");
      Terminal.WriteLine("===============================================================================");

      LocateRunOutcome locateOutcome = await LocateCiRunAsync();

      switch (locateOutcome.Status)
      {
        case LocateRunStatus.GhUnavailable:
          Terminal.WriteErrorLine("Release gate failed: release mode promotes CI-built artifacts and requires the gh CLI. On runners GH_TOKEN is provided by workflow.yml; locally install gh and run 'gh auth login'.");
          AbortPipeline("gh CLI unavailable");
          return;

        case LocateRunStatus.GhFailed:
          Terminal.WriteErrorLine($"Release gate failed: gh run list failed — {locateOutcome.Detail}. If this is transient (network/rate limit), retry; for auth issues run 'gh auth login'.");
          AbortPipeline("gh run list failed");
          return;

        case LocateRunStatus.NoMatchingRun:
          Terminal.WriteErrorLine($"Release gate failed: no successful CI run of workflow.yml exists for commit {locateOutcome.HeadSha}. Only tested CI artifacts are published — this commit must pass CI first. If a run failed, fix and re-run it (gh run rerun <run-id>).");
          AbortPipeline("no successful CI run found");
          return;

        case LocateRunStatus.Found:
          break;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled locate-run status '{locateOutcome.Status}'.");
          AbortPipeline("unhandled locate-run status");
          return;
      }

      Terminal.WriteLine($"Found {locateOutcome.CandidateRuns.Count} candidate CI run(s) for commit {ShortSha(locateOutcome.HeadSha)}.");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 4/6: Download Artifact");
      Terminal.WriteLine("===============================================================================");

      DownloadArtifactOutcome downloadOutcome = await DownloadPackagesArtifactAsync(repoRoot, locateOutcome.CandidateRuns);

      if (downloadOutcome.Status == DownloadArtifactStatus.Exhausted)
      {
        if (downloadOutcome.ExpiredEncounters.Count > 0)
        {
          string expiredDetail = string.Join("; ", downloadOutcome.ExpiredEncounters.Select(encounter => $"run {encounter.RunId} ({encounter.Event}): {string.Join(", ", encounter.ArtifactNames)}"));
          Terminal.WriteErrorLine($"Release gate failed: every candidate CI run's Packages-* artifact has expired — {expiredDetail}. Re-run CI to produce a fresh tested artifact (gh run rerun {downloadOutcome.ExpiredEncounters[0].RunId}).");
        }
        else
        {
          Terminal.WriteErrorLine($"Release gate failed: no candidate CI run for commit {locateOutcome.HeadSha} uploaded a Packages-* artifact. Re-run CI to produce one (gh run rerun {locateOutcome.CandidateRuns[0].DatabaseId}).");
        }

        AbortPipeline("no usable CI artifact found");
        return;
      }

      Terminal.WriteLine($"Downloaded '{downloadOutcome.ArtifactName}' from run {downloadOutcome.Run!.DatabaseId} ({downloadOutcome.Run.Event}).");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 5/6: Verify Package Set");
      Terminal.WriteLine("===============================================================================");

      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      string[] actualNupkgPaths = Directory.Exists(artifactsDir) ? Directory.GetFiles(artifactsDir, "*.nupkg") : [];
      IReadOnlyList<string> actualFileNames = [.. actualNupkgPaths.Select(Path.GetFileName)!];

      PackageSetVerification verification = CiRunPromotion.VerifyPackageSet(actualFileNames, packableProjects, propsVersion!);

      if (!verification.IsMatch)
      {
        if (verification.Missing.Count > 0)
        {
          Terminal.WriteErrorLine($"Release gate failed: downloaded artifact is missing package(s): {string.Join(", ", verification.Missing)}.");
        }

        if (verification.Unexpected.Count > 0)
        {
          Terminal.WriteErrorLine($"Release gate failed: downloaded artifact has unexpected package(s): {string.Join(", ", verification.Unexpected)}.");
        }

        Terminal.WriteErrorLine($"CI run likely predates the version bump — re-run CI on commit {locateOutcome.HeadSha} and retry.");
        AbortPipeline("downloaded package set does not match derived packable set");
        return;
      }

      Terminal.WriteLine("Package set verified.");

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 6/6: Push to NuGet");
      Terminal.WriteLine("===============================================================================");
      await PushPackagesAsync(repoRoot, packableProjects, ApiKey);

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED - Packages published to NuGet.org");
      Terminal.WriteLine("===============================================================================");
    }

    private async Task<bool> RunStepAsync(string stepName, Task<Unit> step)
    {
      await step;

      if (Environment.ExitCode != 0)
      {
        Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} failed".Red());
        return false;
      }

      return true;
    }

    private void AbortPipeline(string reason)
    {
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  Pipeline ABORTED — {reason}");
      Terminal.WriteLine("===============================================================================");
      Environment.ExitCode = 1;
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

    private static string? ReadPropsVersion(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      if (!File.Exists(propsPath))
      {
        return null;
      }

      XDocument document = XDocument.Load(propsPath);
      string? value = document.Descendants("Version").FirstOrDefault()?.Value.Trim();
      return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string ShortSha(string? sha) =>
      string.IsNullOrEmpty(sha) ? "(unknown)" : sha.Length > 7 ? sha[..7] : sha;

  }
}
