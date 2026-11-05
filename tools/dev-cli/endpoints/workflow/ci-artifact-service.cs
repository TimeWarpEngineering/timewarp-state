#region Purpose
// Finds the successful CI run for HEAD and downloads its Packages-* artifact into artifacts/packages.
#endregion

#region Design
// Release promotes tested CI output (Nuru 458 / Ganda 209), so it never rebuilds. gh calls use
// WithNoValidation and read the exit code; a missing gh binary surfaces as Win32Exception -> GhUnavailable.
// Unrecoverable gh failures throw InvalidOperationException; ReleasePipeline turns that into an abort.
#endregion

namespace DevCli.Commands;

internal enum LocateRunStatus
{
  Found,
  GhUnavailable,
  GhFailed,
  NoMatchingRun
}

internal sealed record LocateRunOutcome(LocateRunStatus Status, string HeadSha, IReadOnlyList<CiRunSummary> CandidateRuns, string? Detail);

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

    CommandOutput runListResult;
    try
    {
      runListResult = await Shell.Builder("gh")
        .WithArguments("run", "list", "--workflow", "workflow.yml", "--commit", headSha, "--status", "success", "--json", "databaseId,event,headSha,createdAt")
        .WithNoValidation()
        .CaptureAsync(ct);
    }
    catch (Win32Exception)
    {
      return new LocateRunOutcome(LocateRunStatus.GhUnavailable, headSha, [], null);
    }

    if (runListResult.ExitCode != 0)
    {
      return new LocateRunOutcome(LocateRunStatus.GhFailed, headSha, [], runListResult.Stderr.Trim());
    }

    List<CiRunSummary>? runs = JsonSerializer.Deserialize(runListResult.Stdout, DevCliJsonContext.Default.ListCiRunSummary);
    IReadOnlyList<CiRunSummary> candidateRuns = CiRunPromotion.OrderCandidateRuns(runs ?? [], headSha);

    return candidateRuns.Count == 0
      ? new LocateRunOutcome(LocateRunStatus.NoMatchingRun, headSha, [], null)
      : new LocateRunOutcome(LocateRunStatus.Found, headSha, candidateRuns, null);
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
