#region Purpose
// Proves InspectorLog keeps a bounded, ordered list and tells every circuit when it changes.
#endregion

#region Design
// The log is process-wide and written from request threads, so these tests hit it concurrently and
// check the cap, the sequences, and the Changed notifications for pause and clear.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorLog_Should
{
  public static void KeepUniqueSequencesAndTheCapUnderConcurrentAdds()
  {
    InspectorLog log = new();
    Parallel.For(0, 1000, index => log.Add(Entry("Action" + index)));

    IReadOnlyList<ActionLogEntry> snapshot = log.Snapshot();
    snapshot.Count.ShouldBe(InspectorLog.MaxEntries);
    snapshot.Select(entry => entry.Sequence).Distinct().Count().ShouldBe(InspectorLog.MaxEntries);
    snapshot.Select(entry => entry.Sequence).Min().ShouldBeGreaterThan(0);
  }

  public static void TrimTheOldestEntriesPastTheCap()
  {
    InspectorLog log = new();
    for (int index = 1; index <= InspectorLog.MaxEntries + 10; index++)
    {
      log.Add(Entry("Action" + index));
    }

    IReadOnlyList<ActionLogEntry> snapshot = log.Snapshot();
    snapshot.Count.ShouldBe(InspectorLog.MaxEntries);
    snapshot[0].ActionName.ShouldBe("Action11");
    snapshot[^1].ActionName.ShouldBe("Action" + (InspectorLog.MaxEntries + 10));
  }

  public static void FoldTheLatestSnapshotPerStateUpToASequence()
  {
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":0}""", "init"));
    log.Add(Snap("OtherState", """{"name":"a"}""", "Set"));
    log.Add(Snap("CounterState", """{"count":5}""", "Increment"));
    log.Add(Entry("NoSnapshot"));

    IReadOnlyList<FoldedState> current = log.FoldCurrent();
    current.Count.ShouldBe(2);
    current[0].StateTypeName.ShouldBe("CounterState");
    current[0].Json.ShouldBe("""{"count":5}""");
    current[1].StateTypeName.ShouldBe("OtherState");
    current[1].Json.ShouldBe("""{"name":"a"}""");

    long firstCounter = log.Snapshot()[0].Sequence;
    IReadOnlyList<FoldedState> throughFirst = log.FoldThrough(firstCounter);
    throughFirst.ShouldHaveSingleItem().Json.ShouldBe("""{"count":0}""");
    log.TryGetPreviousSnapshot("CounterState", log.Snapshot()[2].Sequence, out FoldedState? previous).ShouldBeTrue();
    previous.ShouldNotBeNull();
    previous.Json.ShouldBe("""{"count":0}""");
    log.TryGetPreviousSnapshot("CounterState", firstCounter, out _).ShouldBeFalse();
  }

  public static void PreserveTruncationOnTheFoldedSnapshot()
  {
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":1}""", "Increment", truncated: true));
    FoldedState folded = log.FoldCurrent().ShouldHaveSingleItem();
    folded.Truncated.ShouldBeTrue();
    folded.Json.ShouldBe("""{"count":1}""");
  }

  public static void KeepAnIdleStatesSnapshotAfterItsEntryIsTrimmed()
  {
    InspectorLog log = new();
    log.Add(Snap("IdleState", """{"n":1}""", "Init"));
    for (int index = 0; index < InspectorLog.MaxEntries + 5; index++)
    {
      log.Add(Snap("BusyState", """{"n":2}""", "Tick" + index));
    }

    log.FoldCurrent().Select(state => state.StateTypeName).ShouldBe(["BusyState", "IdleState"]);
  }

  public static void LeaveTheCurrentFoldUnchangedUntilPendingRowsFlush()
  {
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":1}""", "First"));
    log.SetPaused(true);
    log.Add(Snap("CounterState", """{"count":2}""", "Second"));
    log.FoldCurrent().ShouldHaveSingleItem().Json.ShouldBe("""{"count":1}""");
    log.SetPaused(false);
    log.FoldCurrent().ShouldHaveSingleItem().Json.ShouldBe("""{"count":2}""");
  }

  public static void RaiseChangedWhenPausedOrClearedSoOtherCircuitsRefresh()
  {
    InspectorLog log = new();
    int changes = 0;
    log.Changed += () => Interlocked.Increment(ref changes);

    log.SetPaused(true);
    changes.ShouldBe(1);
    log.IsPaused.ShouldBeTrue();

    log.SetPaused(true);
    changes.ShouldBe(1);

    log.Add(Entry("Parked"));
    log.SetPaused(false);
    changes.ShouldBe(2);

    log.Clear(raiseChanged: true);
    changes.ShouldBe(3);
    log.Snapshot().ShouldBeEmpty();
  }

  public static void KeepEachStoresFoldWhenTwoStoresInterleave()
  {
    const string prerenderStoreId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    const string interactiveStoreId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":0}""", "init", storeId: prerenderStoreId, render: InspectorCapture.PrerenderRender));
    log.Add(Snap("CounterState", """{"count":0}""", "init", storeId: interactiveStoreId, render: InspectorCapture.InteractiveRender));
    log.Add(Snap("CounterState", """{"count":5}""", "Increment", storeId: interactiveStoreId, render: InspectorCapture.InteractiveRender));
    log.Add(Snap("OtherState", """{"name":"a"}""", "Set", storeId: prerenderStoreId, render: InspectorCapture.PrerenderRender));

    IReadOnlyList<StoreSummary> stores = log.ListStores();
    stores.Count.ShouldBe(2);
    stores[0].StoreId.ShouldBe(interactiveStoreId);
    stores[0].Render.ShouldBe(InspectorCapture.InteractiveRender);
    stores[1].StoreId.ShouldBe(prerenderStoreId);

    FoldedState current = log.FoldCurrent().ShouldHaveSingleItem();
    current.StateTypeName.ShouldBe("CounterState");
    current.Json.ShouldBe("""{"count":5}""");
    current.StoreId.ShouldBe(interactiveStoreId);

    IReadOnlyList<FoldedState> prerenderFold = log.FoldStore(prerenderStoreId);
    prerenderFold.Count.ShouldBe(2);
    prerenderFold.Single(state => state.StateTypeName == "CounterState").Json.ShouldBe("""{"count":0}""");
    prerenderFold.Single(state => state.StateTypeName == "OtherState").Json.ShouldBe("""{"name":"a"}""");

    long increment = log.Snapshot().Single(entry => entry.ActionName == "Increment").Sequence;
    log.TryGetPreviousSnapshot("CounterState", increment, out FoldedState? previous).ShouldBeTrue();
    previous.ShouldNotBeNull();
    previous.StoreId.ShouldBe(interactiveStoreId);
    previous.Json.ShouldBe("""{"count":0}""");
  }

  public static void KeepTheLiveStoreCurrentWhenALaterPrerenderStoreArrives()
  {
    const string interactiveStoreId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    const string prerenderStoreId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":0}""", "init", storeId: interactiveStoreId, render: InspectorCapture.InteractiveRender));
    log.Add(Snap("CounterState", """{"count":5}""", "Increment", storeId: interactiveStoreId, render: InspectorCapture.InteractiveRender));
    // Enhanced navigation prerenders the next page into a new store; the circuit emits no new init.
    log.Add(Snap("CounterState", """{"count":0}""", "init", storeId: prerenderStoreId, render: InspectorCapture.PrerenderRender));

    log.ListStores()[0].StoreId.ShouldBe(prerenderStoreId);
    StoreSelection.Default(log.ListStores())!.StoreId.ShouldBe(interactiveStoreId);
    FoldedState current = log.FoldCurrent().ShouldHaveSingleItem();
    current.StoreId.ShouldBe(interactiveStoreId);
    current.Json.ShouldBe("""{"count":5}""");
  }

  public static void FallBackToTheNewestStoreWhenEveryStoreIsAPrerender()
  {
    const string olderStoreId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    const string newerStoreId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":1}""", "init", storeId: olderStoreId, render: InspectorCapture.PrerenderRender));
    log.Add(Snap("CounterState", """{"count":2}""", "init", storeId: newerStoreId, render: InspectorCapture.PrerenderRender));

    log.FoldCurrent().ShouldHaveSingleItem().StoreId.ShouldBe(newerStoreId);
  }

  public static void FoldThroughOnlyTheSelectedRowsStoreWhenStoresInterleave()
  {
    const string storeA = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    const string storeB = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    InspectorLog log = new();
    log.Add(Snap("CounterState", """{"count":1}""", "A1", storeId: storeA));
    log.Add(Snap("CounterState", """{"count":100}""", "B1", storeId: storeB));
    log.Add(Snap("OtherState", """{"name":"b"}""", "B2", storeId: storeB));
    log.Add(Snap("CounterState", """{"count":2}""", "A2", storeId: storeA));
    log.Add(Snap("CounterState", """{"count":200}""", "B3", storeId: storeB));

    long a2 = log.Snapshot().Single(entry => entry.ActionName == "A2").Sequence;
    FoldedState throughA2 = log.FoldThrough(a2).ShouldHaveSingleItem();
    throughA2.StoreId.ShouldBe(storeA);
    throughA2.Json.ShouldBe("""{"count":2}""");

    long b2 = log.Snapshot().Single(entry => entry.ActionName == "B2").Sequence;
    IReadOnlyList<FoldedState> throughB2 = log.FoldThrough(b2);
    throughB2.Select(state => state.StateTypeName).ShouldBe(["CounterState", "OtherState"]);
    throughB2.ShouldAllBe(state => state.StoreId == storeB);
    throughB2[0].Json.ShouldBe("""{"count":100}""");
  }

  private static ActionLogEntry Snap
  (
    string stateTypeName,
    string json,
    string actionName,
    bool truncated = false,
    string storeId = "",
    string render = ""
  ) =>
    new()
    {
      Name = stateTypeName + "." + actionName,
      ActionName = actionName,
      StateTypeName = stateTypeName,
      Status = "Ok",
      Sequence = 0,
      DurationMilliseconds = 1,
      StartTimeUtc = DateTime.UtcNow,
      SnapshotJson = json,
      SnapshotKind = InspectorCapture.SnapshotEventName,
      SnapshotTruncated = truncated,
      StoreId = storeId,
      Render = render
    };

  private static ActionLogEntry Entry(string actionName) =>
    new()
    {
      Name = "CounterState." + actionName,
      ActionName = actionName,
      StateTypeName = "CounterState",
      Status = "Ok",
      Sequence = 0,
      DurationMilliseconds = 1,
      StartTimeUtc = DateTime.UtcNow
    };
}
