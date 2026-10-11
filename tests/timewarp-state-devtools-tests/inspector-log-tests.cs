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

    log.FlushPending();
    changes.ShouldBe(3);

    log.Clear(raiseChanged: true);
    changes.ShouldBe(4);
    log.Snapshot().ShouldBeEmpty();
  }

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
