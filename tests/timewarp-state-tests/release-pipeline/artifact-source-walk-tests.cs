#region Purpose
// Proves the release locate-run walk promotes the green commit behind kanban-only commits and refuses
// when a commit in between changes a shipping path (task 133).
#endregion

#region Design
// ArtifactSourceWalk is the same type `dev workflow` (release mode) runs. FakeCommitCiLookup stands in
// for gh and git: a fake run list per commit and fake changed paths per commit. It records which
// commits were asked for runs, so a test can prove a green HEAD costs one lookup and that the walk stops.
// The history mirrors 2026-10-31: merge commit d120f3a with a green push run, then a land-kanban
// commit e05df14 with no run.
#endregion

namespace ArtifactSourceWalkTests;

public class Should_
{
  private const string LandKanbanSha = "e05df14500000000000000000000000000000000";
  private const string SecondLandKanbanSha = "f00dfeed00000000000000000000000000000000";
  private const string MergeSha = "d120f3a000000000000000000000000000000000";
  private const string OlderMergeSha = "a1b2c3d400000000000000000000000000000000";

  public async Task Return_Head_When_Head_Has_A_Successful_Run()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Runs[MergeSha] = [Run(101, "push", MergeSha)];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(MergeSha, [MergeSha, OlderMergeSha], lookup, CancellationToken.None);

    result.Verdict.Ok.ShouldBeTrue();
    result.Verdict.TargetSha.ShouldBe(MergeSha);
    result.Verdict.SkippedCommitCount.ShouldBe(0);
    result.CandidateRuns.Select(run => run.DatabaseId).ShouldBe([101L]);
    lookup.RunQueries.ShouldBe([MergeSha]);
    lookup.PathQueries.ShouldBeEmpty();
  }

  public async Task Promote_The_Merge_Run_When_Head_Is_A_Land_Kanban_Commit()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Paths[LandKanbanSha] = ["kanban/done/132-update-nuget-packages/task.md", "kanban/in-progress/132-update-nuget-packages/task.md"];
    lookup.Runs[MergeSha] = [Run(201, "push", MergeSha)];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(LandKanbanSha, [LandKanbanSha, MergeSha, OlderMergeSha], lookup, CancellationToken.None);

    result.Verdict.Ok.ShouldBeTrue();
    result.Verdict.TargetSha.ShouldBe(MergeSha);
    result.Verdict.RunId.ShouldBe(201L);
    result.Verdict.SkippedCommitCount.ShouldBe(1);
    result.CandidateRuns.Select(run => run.DatabaseId).ShouldBe([201L]);
    result.ShippingCommitSha.ShouldBeNull();
    lookup.RunQueries.ShouldBe([LandKanbanSha, MergeSha]);
  }

  public async Task Skip_Several_Kanban_Only_Commits_And_An_Empty_Commit()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Paths[SecondLandKanbanSha] = [];
    lookup.Paths[LandKanbanSha] = ["kanban/done/132-update-nuget-packages/task.md"];
    lookup.Runs[MergeSha] = [Run(301, "push", MergeSha)];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(SecondLandKanbanSha, [SecondLandKanbanSha, LandKanbanSha, MergeSha], lookup, CancellationToken.None);

    result.Verdict.Ok.ShouldBeTrue();
    result.Verdict.TargetSha.ShouldBe(MergeSha);
    result.Verdict.SkippedCommitCount.ShouldBe(2);
  }

  public async Task Order_Push_Runs_First_And_Drop_Pull_Request_Runs_And_Other_Commits()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Paths[LandKanbanSha] = ["kanban/done/132-update-nuget-packages/task.md"];
    lookup.Runs[MergeSha] =
    [
      Run(401, "workflow_dispatch", MergeSha),
      Run(402, "pull_request", MergeSha),
      Run(403, "push", MergeSha),
      Run(404, "push", OlderMergeSha),
    ];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(LandKanbanSha, [LandKanbanSha, MergeSha], lookup, CancellationToken.None);

    result.Verdict.RunId.ShouldBe(403L);
    result.CandidateRuns.Select(run => run.DatabaseId).ShouldBe([403L, 401L]);
  }

  public async Task Refuse_When_A_Commit_After_The_Green_Run_Changes_A_Shipping_Path()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Paths[LandKanbanSha] = ["kanban/done/132-update-nuget-packages/task.md"];
    lookup.Paths[MergeSha] = ["kanban/done/132-update-nuget-packages/task.md", "source/timewarp-state/store.cs"];
    lookup.Runs[OlderMergeSha] = [Run(501, "push", OlderMergeSha)];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(LandKanbanSha, [LandKanbanSha, MergeSha, OlderMergeSha], lookup, CancellationToken.None);

    result.Verdict.Ok.ShouldBeFalse();
    result.CandidateRuns.ShouldBeEmpty();
    result.ShippingCommitSha.ShouldBe(MergeSha);
    result.ShippingPath.ShouldBe("source/timewarp-state/store.cs");
    lookup.RunQueries.ShouldBe([LandKanbanSha, MergeSha]);
  }

  public async Task Refuse_When_A_Rename_Moves_A_Shipping_File_Into_Kanban()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Paths[LandKanbanSha] = ["kanban/notes/store.cs", "source/timewarp-state/store.cs"];
    lookup.Runs[MergeSha] = [Run(601, "push", MergeSha)];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(LandKanbanSha, [LandKanbanSha, MergeSha], lookup, CancellationToken.None);

    result.Verdict.Ok.ShouldBeFalse();
    result.ShippingPath.ShouldBe("source/timewarp-state/store.cs");
  }

  public async Task Refuse_When_The_Walk_Runs_Out_Of_Commits()
  {
    FakeCommitCiLookup lookup = new();
    lookup.Paths[LandKanbanSha] = ["kanban/done/132-update-nuget-packages/task.md"];
    lookup.Paths[MergeSha] = ["kanban/done/131-something/task.md"];

    ArtifactSourceWalkResult result = await ArtifactSourceWalk.RunAsync(LandKanbanSha, [LandKanbanSha, MergeSha], lookup, CancellationToken.None);

    result.Verdict.Ok.ShouldBeFalse();
    result.Verdict.Reason.ShouldNotBeNull().ShouldContain(LandKanbanSha);
    result.ShippingCommitSha.ShouldBeNull();
  }

  private static CiRunSummary Run(long id, string eventName, string headSha) =>
    new() { DatabaseId = id, Event = eventName, HeadSha = headSha, CreatedAt = $"2026-10-31T00:00:{id % 60:00}Z" };

  private sealed class FakeCommitCiLookup : ICommitCiLookup
  {
    public Dictionary<string, IReadOnlyList<CiRunSummary>> Runs { get; } = [];

    public Dictionary<string, IReadOnlyList<string>> Paths { get; } = [];

    public List<string> RunQueries { get; } = [];

    public List<string> PathQueries { get; } = [];

    public Task<IReadOnlyList<CiRunSummary>> GetSuccessfulRunsAsync(string sha, CancellationToken ct)
    {
      RunQueries.Add(sha);
      return Task.FromResult(Runs.TryGetValue(sha, out IReadOnlyList<CiRunSummary>? runs) ? runs : []);
    }

    public Task<IReadOnlyList<string>> GetChangedPathsAsync(string sha, CancellationToken ct)
    {
      PathQueries.Add(sha);
      return Task.FromResult(Paths.TryGetValue(sha, out IReadOnlyList<string>? paths)
        ? paths
        : throw new InvalidOperationException($"No fake paths for {sha}."));
    }
  }
}
