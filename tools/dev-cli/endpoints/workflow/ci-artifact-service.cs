#region Purpose
// Finds the successful CI run to promote (HEAD's, or that of the green commit HEAD is only kanban-only
// commits past) and downloads its Packages-* artifact into artifacts/packages.
#endregion

#region Design
// Release promotes tested CI output (Nuru 458 / Ganda 209), so it never rebuilds. gh calls use
// WithNoValidation and read the exit code; a missing gh binary surfaces as Win32Exception -> GhUnavailable.
// Unrecoverable gh failures throw InvalidOperationException; ReleasePipeline turns that into an abort.
// Locate-run walks first-parent history through ArtifactSourceWalk (task 133); GhGitCommitCiLookup is
// its gh/git side. A failed gh run list throws GhRunListException so the walk ends and locate-run
// reports GhFailed, as it did before the walk existed.
#endregion

namespace DevCli.Commands;

internal enum LocateRunStatus
{
  Found,
  GhUnavailable,
  GhFailed,
  NoMatchingRun
}

/// <summary>
/// Locate-run result. <see cref="TargetSha"/> is the commit whose artifact is promoted: HEAD, or the
/// green commit HEAD is <see cref="SkippedCommitCount"/> kanban-only commits past. On failure it is HEAD.
/// </summary>
internal sealed record LocateRunOutcome
(
  LocateRunStatus Status,
  string HeadSha,
  string TargetSha,
  int SkippedCommitCount,
  IReadOnlyList<CiRunSummary> CandidateRuns,
  string? Detail
)
{
  public static LocateRunOutcome Failed(LocateRunStatus status, string headSha, string? detail) =>
    new(status, headSha, headSha, 0, [], detail);
}

internal enum DownloadArtifactStatus
{
  Downloaded,
  Exhausted
}

internal sealed record DownloadArtifactOutcome(DownloadArtifactStatus Status, CiRunSummary? Run, string? ArtifactName, IReadOnlyList<ExpiredArtifactEncounter> ExpiredEncounters);

internal sealed record ExpiredArtifactEncounter(long RunId, string Event, IReadOnlyList<string> ArtifactNames);

internal sealed class CiArtifactService
{
  private readonly ITerminal Terminal;

  public CiArtifactService(ITerminal terminal)
  {
    Terminal = terminal;
  }

  public static async Task<LocateRunOutcome> LocateCiRunAsync(CancellationToken ct)
  {
    CommandOutput headResult = await ReleaseGitGate.GitAsync(ct, "rev-parse", "HEAD");

    if (headResult.ExitCode != 0)
    {
      throw new InvalidOperationException($"Could not determine HEAD commit: {headResult.Stderr.Trim()}");
    }

    string headSha = headResult.Stdout.Trim();

    CommandOutput revListResult = await ReleaseGitGate.GitAsync(ct, "rev-list", "--first-parent", $"--max-count={ReleaseGuard.ReleaseTargetWalkLimit}", headSha);

    if (revListResult.ExitCode != 0)
    {
      throw new InvalidOperationException($"Could not list first-parent history from HEAD: {revListResult.Stderr.Trim()}");
    }

    string[] firstParentShas = revListResult.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    ArtifactSourceWalkResult walk;
    try
    {
      walk = await ArtifactSourceWalk.RunAsync(headSha, firstParentShas, new GhGitCommitCiLookup(), ct);
    }
    catch (Win32Exception)
    {
      return LocateRunOutcome.Failed(LocateRunStatus.GhUnavailable, headSha, null);
    }
    catch (GhRunListException exception)
    {
      return LocateRunOutcome.Failed(LocateRunStatus.GhFailed, headSha, exception.Message);
    }

    if (!walk.Verdict.Ok)
    {
      string? detail = walk.ShippingCommitSha is null
        ? null
        : $"Commit {walk.ShippingCommitSha} changes {walk.ShippingPath}, outside {ReleaseGuard.NonShippingPathPrefix}, so no earlier green run can stand in for it.";
      return LocateRunOutcome.Failed(LocateRunStatus.NoMatchingRun, headSha, detail);
    }

    return new LocateRunOutcome(LocateRunStatus.Found, headSha, walk.Verdict.TargetSha!, walk.Verdict.SkippedCommitCount, walk.CandidateRuns, null);
  }

  public async Task<DownloadArtifactOutcome> DownloadPackagesArtifactAsync(
    string repoRoot,
    IReadOnlyList<CiRunSummary> candidateRuns,
    CancellationToken ct)
  {
    string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
    List<ExpiredArtifactEncounter> expiredEncounters = [];

    foreach (CiRunSummary run in candidateRuns)
    {
      CommandOutput artifactsResult = await Shell.Builder("gh")
        .WithArguments("api", $"repos/{{owner}}/{{repo}}/actions/runs/{run.DatabaseId}/artifacts")
        .WithNoValidation()
        .CaptureAsync(ct);

      if (artifactsResult.ExitCode != 0)
      {
        throw new InvalidOperationException($"Failed to list artifacts for run {run.DatabaseId}: {artifactsResult.Stderr.Trim()}");
      }

      RunArtifactListResponse? artifactList = JsonSerializer.Deserialize(artifactsResult.Stdout, DevCliJsonContext.Default.RunArtifactListResponse);
      PackagesArtifactOutcome selectOutcome = CiRunPromotion.SelectPackagesArtifact(artifactList?.Artifacts ?? []);

      if (selectOutcome.Status == PackagesArtifactStatus.Expired)
      {
        expiredEncounters.Add(new ExpiredArtifactEncounter(run.DatabaseId, run.Event, selectOutcome.ExpiredNames));
        continue;
      }

      if (selectOutcome.Status == PackagesArtifactStatus.NoneMatching)
      {
        continue;
      }

      RunArtifact artifact = selectOutcome.Artifact!;

      if (Directory.Exists(artifactsDir))
      {
        Directory.Delete(artifactsDir, recursive: true);
      }

      Directory.CreateDirectory(artifactsDir);

      Terminal.WriteLine($"Downloading artifact '{artifact.Name}' from run {run.DatabaseId} ({run.Event})...");

      int exitCode = await Shell.Builder("gh")
        .WithArguments("run", "download", run.DatabaseId.ToString(CultureInfo.InvariantCulture), "--name", artifact.Name, "--dir", artifactsDir)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        throw new InvalidOperationException($"Failed to download artifact '{artifact.Name}' from run {run.DatabaseId} (gh exit {exitCode}).");
      }

      return new DownloadArtifactOutcome(DownloadArtifactStatus.Downloaded, run, artifact.Name, expiredEncounters);
    }

    return new DownloadArtifactOutcome(DownloadArtifactStatus.Exhausted, null, null, expiredEncounters);
  }
}

/// <summary>Thrown by <see cref="GhGitCommitCiLookup"/> when <c>gh run list</c> exits non-zero.</summary>
internal sealed class GhRunListException : Exception
{
  public GhRunListException(string message) : base(message) { }

  public GhRunListException() { }

  public GhRunListException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>The real <see cref="ICommitCiLookup"/>: <c>gh run list</c> per commit and <c>git diff</c> for paths.</summary>
internal sealed class GhGitCommitCiLookup : ICommitCiLookup
{
  public async Task<IReadOnlyList<CiRunSummary>> GetSuccessfulRunsAsync(string sha, CancellationToken ct)
  {
    CommandOutput runListResult = await Shell.Builder("gh")
      .WithArguments("run", "list", "--workflow", "workflow.yml", "--commit", sha, "--status", "success", "--json", "databaseId,event,headSha,createdAt")
      .WithNoValidation()
      .CaptureAsync(ct);

    if (runListResult.ExitCode != 0)
    {
      throw new GhRunListException(runListResult.Stderr.Trim());
    }

    return JsonSerializer.Deserialize(runListResult.Stdout, DevCliJsonContext.Default.ListCiRunSummary) ?? [];
  }

  public async Task<IReadOnlyList<string>> GetChangedPathsAsync(string sha, CancellationToken ct)
  {
    // --no-renames lists both sides of a rename, so a move from source/ into kanban/ still shows source/.
    CommandOutput diffResult = await ReleaseGitGate.GitAsync(ct, "diff", "--name-only", "--no-renames", $"{sha}^1", sha);

    if (diffResult.ExitCode != 0)
    {
      throw new InvalidOperationException($"Could not list the paths changed by {sha}: {diffResult.Stderr.Trim()}");
    }

    return diffResult.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
  }
}
