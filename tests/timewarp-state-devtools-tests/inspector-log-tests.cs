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

  private static ActionLogEntry Snap(string stateTypeName, string json, string actionName, bool truncated = false) =>
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
      SnapshotTruncated = truncated
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
