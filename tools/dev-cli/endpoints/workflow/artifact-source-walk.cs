#region Purpose
// Finds the commit whose CI artifact a release promotes: HEAD, or the newest green first-parent commit
// that HEAD is only kanban-only commits past (task 133).
#endregion

#region Design
// ganda lands the kanban move as its own commit after a merge, and the workflow.yml push filter skips
// kanban/**, so master HEAD often has no CI run. `dev release` (TimeWarp.Nuru.DevCli, Nuru task 543)
// already tags the green commit behind such commits; the release event then checks out that tag and
// this walk stops at HEAD. The walk matters for break-glass dispatches from master, where HEAD can be
// a land-kanban commit.
// The decision rules are the package's: CiRunPromotion.OrderCandidateRuns picks the candidate runs of a
// commit, ReleaseGuard.FindShippingPath ends the walk at the first path outside kanban/, and
// ReleaseGuard.SelectReleaseTarget gives the verdict and the operator-facing refusal. This file only
// drives the walk, one commit at a time, so a green HEAD costs one lookup. ICommitCiLookup carries the
// gh and git calls so tests can feed a fake run list.
#endregion

namespace DevCli.Commands;

/// <summary>The gh and git reads the artifact-source walk needs for one commit.</summary>
internal interface ICommitCiLookup
{
  /// <summary>Successful <c>workflow.yml</c> runs whose head is <paramref name="sha"/>, as gh lists them.</summary>
  Task<IReadOnlyList<CiRunSummary>> GetSuccessfulRunsAsync(string sha, CancellationToken ct);

  /// <summary>Paths <paramref name="sha"/> changes against its first parent.</summary>
  Task<IReadOnlyList<string>> GetChangedPathsAsync(string sha, CancellationToken ct);
}

/// <summary>
/// The walk's verdict, plus the ordered candidate runs of the target commit when the verdict is OK.
/// When the walk ended at a commit that changes a path outside kanban/, <see cref="ShippingCommitSha"/>
/// and <see cref="ShippingPath"/> name that commit and its first such path.
/// </summary>
internal sealed record ArtifactSourceWalkResult
(
  ReleaseTargetVerdict Verdict,
  IReadOnlyList<CiRunSummary> CandidateRuns,
  string? ShippingCommitSha,
  string? ShippingPath
);

internal static class ArtifactSourceWalk
{
  /// <summary>
  /// Walks <paramref name="firstParentShas"/> (HEAD first) until a commit has a successful run or
  /// changes a path outside kanban/.
  /// </summary>
  public static async Task<ArtifactSourceWalkResult> RunAsync
  (
    string headSha,
    IReadOnlyList<string> firstParentShas,
    ICommitCiLookup lookup,
    CancellationToken ct
  )
  {
    ArgumentNullException.ThrowIfNull(headSha);
    ArgumentNullException.ThrowIfNull(firstParentShas);
    ArgumentNullException.ThrowIfNull(lookup);

    List<ReleaseTargetCommit> commits = [];
    IReadOnlyList<CiRunSummary> targetRuns = [];
    string? shippingCommitSha = null;
    string? shippingPath = null;

    foreach (string sha in firstParentShas)
    {
      IReadOnlyList<CiRunSummary> runs = await lookup.GetSuccessfulRunsAsync(sha, ct);
      IReadOnlyList<CiRunSummary> candidateRuns = CiRunPromotion.OrderCandidateRuns(runs, sha);

      if (candidateRuns.Count > 0)
      {
        commits.Add(new ReleaseTargetCommit(sha, [], candidateRuns[0].DatabaseId));
        targetRuns = candidateRuns;
        break;
      }

      IReadOnlyList<string> changedPaths = await lookup.GetChangedPathsAsync(sha, ct);
      commits.Add(new ReleaseTargetCommit(sha, changedPaths, null));

      shippingPath = ReleaseGuard.FindShippingPath(changedPaths);
      if (shippingPath is not null)
      {
        shippingCommitSha = sha;
        break;
      }
    }

    ReleaseTargetVerdict verdict = ReleaseGuard.SelectReleaseTarget(headSha, commits);
    return new ArtifactSourceWalkResult(verdict, verdict.Ok ? targetRuns : [], shippingCommitSha, shippingPath);
  }
}
