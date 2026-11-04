#region Purpose
// Release half of the workflow command: tag pin, CI run lookup, artifact download, and NuGet push.
#endregion

#region Design
// Partial of WorkflowCommand.Handler. PR steps stay in workflow-command.cs. Outcomes stay nested so the release methods keep their private types.
#endregion

namespace DevCli.Commands;

internal sealed partial class WorkflowCommand
{
  internal sealed partial class Handler
  {
    private static async Task<TagPinOutcome> CheckTagPinAsync(string version)
    {
      string tag = $"v{version}";

      CommandOutput verifyResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "-q", "--verify", $"refs/tags/{tag}")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (verifyResult.ExitCode != 0)
      {
        if (!string.IsNullOrWhiteSpace(verifyResult.Stderr))
        {
          return new TagPinOutcome(TagPinStatus.GitError, null, null, verifyResult.Stderr.Trim());
        }

        return new TagPinOutcome(TagPinStatus.NoTag, null, null, null);
      }

      CommandOutput tagCommitResult = await Shell.Builder("git")
        .WithArguments("rev-parse", $"{tag}^{{commit}}")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (tagCommitResult.ExitCode != 0)
      {
        return new TagPinOutcome(TagPinStatus.GitError, null, null, tagCommitResult.Stderr.Trim());
      }

      CommandOutput headResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "HEAD")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (headResult.ExitCode != 0)
      {
        return new TagPinOutcome(TagPinStatus.GitError, null, null, headResult.Stderr.Trim());
      }

      string tagCommit = tagCommitResult.Stdout.Trim();
      string headCommit = headResult.Stdout.Trim();

      return string.Equals(tagCommit, headCommit, StringComparison.Ordinal)
        ? new TagPinOutcome(TagPinStatus.Match, tagCommit, headCommit, null)
        : new TagPinOutcome(TagPinStatus.Mismatch, tagCommit, headCommit, null);
    }

    private async Task<AncestorCheckOutcome> CheckHeadAncestorOfMasterAsync()
    {
      string masterRef = "origin/master";

      CommandOutput verifyResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "--verify", "origin/master")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (verifyResult.ExitCode != 0)
      {
        masterRef = "master";

        CommandOutput fallbackVerifyResult = await Shell.Builder("git")
          .WithArguments("rev-parse", "--verify", "master")
          .WithNoValidation()
          .CaptureAsync(CancellationToken.None);

        if (fallbackVerifyResult.ExitCode != 0)
        {
          return new AncestorCheckOutcome(AncestorCheckStatus.MasterUnresolvable, null);
        }

        Terminal.WriteLine("origin/master not found; using local master.");
      }

      CommandOutput ancestorResult = await Shell.Builder("git")
        .WithArguments("merge-base", "--is-ancestor", "HEAD", masterRef)
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (ancestorResult.ExitCode == 0)
      {
        return new AncestorCheckOutcome(AncestorCheckStatus.Ancestor, null);
      }

      if (ancestorResult.ExitCode == 1)
      {
        return new AncestorCheckOutcome(AncestorCheckStatus.NotAncestor, null);
      }

      return new AncestorCheckOutcome(AncestorCheckStatus.GitError, ancestorResult.Stderr.Trim());
    }

    private static async Task<LocateRunOutcome> LocateCiRunAsync()
    {
      CommandOutput headResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "HEAD")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

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
          .CaptureAsync(CancellationToken.None);
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

    private async Task<DownloadArtifactOutcome> DownloadPackagesArtifactAsync(string repoRoot, IReadOnlyList<CiRunSummary> candidateRuns)
    {
      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      List<ExpiredArtifactEncounter> expiredEncounters = [];

      foreach (CiRunSummary run in candidateRuns)
      {
        CommandOutput artifactsResult = await Shell.Builder("gh")
          .WithArguments("api", $"repos/{{owner}}/{{repo}}/actions/runs/{run.DatabaseId}/artifacts")
          .WithNoValidation()
          .CaptureAsync(CancellationToken.None);

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
          .RunAsync();

        if (exitCode != 0)
        {
          throw new InvalidOperationException($"Failed to download artifact '{artifact.Name}' from run {run.DatabaseId}!");
        }

        return new DownloadArtifactOutcome(DownloadArtifactStatus.Downloaded, run, artifact.Name, expiredEncounters);
      }

      return new DownloadArtifactOutcome(DownloadArtifactStatus.Exhausted, null, null, expiredEncounters);
    }

    private async Task PushPackagesAsync(string repoRoot, IReadOnlyList<PackableProject> projects, string? apiKey)
    {
      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      string? version = ReadPropsVersion(repoRoot);

      if (string.IsNullOrEmpty(version))
      {
        throw new InvalidOperationException("Could not determine version for push");
      }

      HashSet<string> expectedNupkgFileNames = [.. projects.Select(project => $"{project.PackageId}.{version}.nupkg")];

      string[] actualNupkgFiles = Directory.Exists(artifactsDir)
        ? Directory.GetFiles(artifactsDir, $"*.{version}.nupkg")
        : [];

      List<string> unexpectedNupkgFileNames = [];
      foreach (string filePath in actualNupkgFiles)
      {
        string fileName = Path.GetFileName(filePath);
        if (!expectedNupkgFileNames.Contains(fileName))
        {
          unexpectedNupkgFileNames.Add(fileName);
        }
      }

      if (unexpectedNupkgFileNames.Count > 0)
      {
        throw new InvalidOperationException($"Unexpected package(s) in {artifactsDir} not in the derived packable set: {string.Join(", ", unexpectedNupkgFileNames)}");
      }

      foreach (PackableProject project in projects)
      {
        string nupkgPath = Path.Combine(artifactsDir, $"{project.PackageId}.{version}.nupkg");

        if (!File.Exists(nupkgPath))
        {
          throw new FileNotFoundException($"Package not found: {nupkgPath}");
        }

        Terminal.WriteLine($"Pushing {project.PackageId}.{version}.nupkg...");

        List<string> args = ["nuget", "push", nupkgPath, "--source", "https://api.nuget.org/v3/index.json", "--skip-duplicate"];

        if (!string.IsNullOrEmpty(apiKey))
        {
          args.AddRange(["--api-key", apiKey]);
        }

        int exitCode = await Shell.Builder("dotnet")
          .WithArguments([.. args])
          .WithWorkingDirectory(repoRoot)
          .RunAsync();

        if (exitCode != 0)
        {
          throw new InvalidOperationException($"Failed to push {project.PackageId}!");
        }
      }

      Terminal.WriteLine("\nAll packages pushed successfully!");
    }

    private enum AncestorCheckStatus
    {
      Ancestor,
      NotAncestor,
      MasterUnresolvable,
      GitError
    }

    private sealed record AncestorCheckOutcome(AncestorCheckStatus Status, string? Detail);

    private enum TagPinStatus
    {
      NoTag,
      Match,
      Mismatch,
      GitError
    }

    private sealed record TagPinOutcome(TagPinStatus Status, string? TagCommit, string? HeadCommit, string? Detail);

    private enum LocateRunStatus
    {
      Found,
      GhUnavailable,
      GhFailed,
      NoMatchingRun
    }

    private sealed record LocateRunOutcome(LocateRunStatus Status, string HeadSha, IReadOnlyList<CiRunSummary> CandidateRuns, string? Detail);

    private enum DownloadArtifactStatus
    {
      Downloaded,
      Exhausted
    }

    private sealed record DownloadArtifactOutcome(DownloadArtifactStatus Status, CiRunSummary? Run, string? ArtifactName, IReadOnlyList<ExpiredArtifactEncounter> ExpiredEncounters);

    private sealed record ExpiredArtifactEncounter(long RunId, string Event, IReadOnlyList<string> ArtifactNames);
  }
}
