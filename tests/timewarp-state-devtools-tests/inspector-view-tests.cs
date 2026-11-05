#region Purpose
// Proves InspectorView reads the log the way the Inspector page did before the split.
#endregion

#region Design
// InspectorView is the page's only query: filtered rows, the selected row with its diff, the shown
// store, and the fold. These checks drive the state through actions so the view sees real choices.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorView_Should
{
  private const string StoreA = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

  public static async Task FilterRowsAndPairTheSelectionWithItsDiff()
  {
    await using WebApplicationFactory<Program> factory = new();
    InspectorLog log = factory.Services.GetRequiredService<InspectorLog>();
    log.Add(Entry("Increment", """{"Count":1}"""));
    log.Add(Entry("Increment", """{"Count":2}"""));
    log.Add(Entry("Reset", """{"Count":0}"""));
    long second = log.Snapshot()[1].Sequence;

    using IServiceScope scope = factory.Services.CreateScope();
    ISender<ClientPipeline> sender = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
    IStore store = scope.ServiceProvider.GetRequiredService<IStore>();

    InspectorView view = InspectorView.Create(log, store.GetState<InspectorState>());
    view.Entries.Count.ShouldBe(3);
    view.Selected.ShouldBeNull();
    view.SelectedDiff.ShouldBeNull();
    view.StateAfterAction.ShouldBeFalse();
    view.StoreId.ShouldBe(StoreA);
    view.Folded.Single().Json.ShouldBe("""{"Count":0}""");

    await sender.Send(new InspectorState.SetFilterActionSet.Action("  incr  "));
    await sender.Send(new InspectorState.SelectEntryActionSet.Action(second));
    view = InspectorView.Create(log, store.GetState<InspectorState>());
    view.Entries.Select(entry => entry.ActionName).ShouldBe(["Increment", "Increment"]);
    view.Selected.ShouldNotBeNull().Sequence.ShouldBe(second);
    InspectorView.ActionDiff diff = view.SelectedDiff.ShouldNotBeNull();
    diff.Entry.ShouldBeSameAs(view.Selected);
    diff.Result.Status.ShouldBe(StateDiff.Status.Changes);
    view.StateAfterAction.ShouldBeTrue();
    view.Folded.Single().Json.ShouldBe("""{"Count":2}""");
    view.States.ShouldBeEmpty();
  }

  public static void KeepEveryStateWhenTheFilterMatchesTheStoreOrIsEmpty()
  {
    FoldedState counter = new() { StateTypeName = "CounterState", Sequence = 1, Json = "{}", Truncated = false };
    FoldedState weather = new() { StateTypeName = "WeatherState", Sequence = 2, Json = "{}", Truncated = false };
    IReadOnlyList<FoldedState> folded = [counter, weather];

    InspectorView.FilterStates(folded, [], StoreA, "").ShouldBeSameAs(folded);
    InspectorView.FilterStates(folded, [], StoreA, "weather").ShouldBe([weather]);
    InspectorView.FilterStates(folded, [], StoreA, "nothing").ShouldBeEmpty();
  }

  private static ActionLogEntry Entry(string actionName, string snapshotJson) =>
    new()
    {
      Name = "CounterState." + actionName,
      ActionName = actionName,
      StateTypeName = "CounterState",
      Status = "Ok",
      Sequence = 0,
      DurationMilliseconds = 1,
      StartTimeUtc = DateTime.UtcNow,
      SnapshotJson = snapshotJson,
      SnapshotKind = InspectorCapture.SnapshotEventName,
      StoreId = StoreA,
      Render = "interactive"
    };
}
