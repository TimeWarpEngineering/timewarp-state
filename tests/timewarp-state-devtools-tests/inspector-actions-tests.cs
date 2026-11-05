#region Purpose
// Proves the Inspector's single-dispatch actions: row selection, store picker, theme cycle, and node toggles.
#endregion

#region Design
// The page only dispatches, so each decision the page used to make is checked here through the
// store. A real DevTools host supplies the singleton InspectorLog and the scoped store. Theme
// actions run without a circuit; ApplyThemeAsync swallows the missing JS runtime, as in prerender.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorActions_Should
{
  private const string StoreA = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
  private const string StoreB = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

  public static async Task PinTheSelectedRowsStoreInOneAction()
  {
    await using WebApplicationFactory<Program> factory = new();
    InspectorLog log = factory.Services.GetRequiredService<InspectorLog>();
    log.Add(Entry("Increment", StoreA));
    log.Add(Entry("Untagged", ""));
    long first = log.Snapshot()[0].Sequence;
    long second = log.Snapshot()[1].Sequence;

    using IServiceScope scope = factory.Services.CreateScope();
    ISender<ClientPipeline> sender = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
    IStore store = scope.ServiceProvider.GetRequiredService<IStore>();

    await sender.Send(new InspectorState.SelectEntryActionSet.Action(first));
    store.GetState<InspectorState>().SelectedSequence.ShouldBe(first);
    store.GetState<InspectorState>().SelectedStoreId.ShouldBe(StoreA);

    await sender.Send(new InspectorState.SelectEntryActionSet.Action(second));
    store.GetState<InspectorState>().SelectedStoreId.ShouldBe("");

    await sender.Send(new InspectorState.SelectStoreActionSet.Action(StoreB));
    await sender.Send(new InspectorState.SelectEntryActionSet.Action(0));
    store.GetState<InspectorState>().SelectedSequence.ShouldBe(0);
    store.GetState<InspectorState>().SelectedStoreId.ShouldBe(StoreB);

    await sender.Send(new InspectorState.SelectEntryActionSet.Action(9999));
    store.GetState<InspectorState>().SelectedSequence.ShouldBe(9999);
    store.GetState<InspectorState>().SelectedStoreId.ShouldBe(StoreB);
  }

  public static async Task DecodeTheStorePickerAndClearTheSelection()
  {
    await using WebApplicationFactory<Program> factory = new();
    InspectorLog log = factory.Services.GetRequiredService<InspectorLog>();
    log.Add(Entry("Increment", StoreA));
    long sequence = log.Snapshot()[0].Sequence;

    using IServiceScope scope = factory.Services.CreateScope();
    ISender<ClientPipeline> sender = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
    IStore store = scope.ServiceProvider.GetRequiredService<IStore>();

    await sender.Send(new InspectorState.SelectEntryActionSet.Action(sequence));
    await sender.Send(new InspectorState.SelectStoreOptionActionSet.Action(StoreB));
    store.GetState<InspectorState>().SelectedStoreId.ShouldBe(StoreB);
    store.GetState<InspectorState>().SelectedSequence.ShouldBe(0);

    await sender.Send(new InspectorState.SelectEntryActionSet.Action(sequence));
    await sender.Send(new InspectorState.SelectStoreOptionActionSet.Action(InspectorState.StoreOptionUntagged));
    store.GetState<InspectorState>().SelectedStoreId.ShouldBe("");
    store.GetState<InspectorState>().SelectedSequence.ShouldBe(0);

    await sender.Send(new InspectorState.SelectStoreOptionActionSet.Action(InspectorState.StoreOptionFollow));
    store.GetState<InspectorState>().SelectedStoreId.ShouldBeNull();

    await sender.Send(new InspectorState.SelectStoreActionSet.Action(StoreA));
    await sender.Send(new InspectorState.SelectStoreOptionActionSet.Action(""));
    store.GetState<InspectorState>().SelectedStoreId.ShouldBeNull();
  }

  public static async Task CycleTheThemeAndToggleNodes()
  {
    await using WebApplicationFactory<Program> factory = new();
    using IServiceScope scope = factory.Services.CreateScope();
    ISender<ClientPipeline> sender = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
    IStore store = scope.ServiceProvider.GetRequiredService<IStore>();

    store.GetState<InspectorState>().Theme.ShouldBe(InspectorState.ThemeSystem);
    await sender.Send(new InspectorState.CycleThemeActionSet.Action());
    store.GetState<InspectorState>().Theme.ShouldBe(InspectorState.ThemeDark);
    await sender.Send(new InspectorState.CycleThemeActionSet.Action());
    store.GetState<InspectorState>().Theme.ShouldBe(InspectorState.ThemeLight);
    await sender.Send(new InspectorState.CycleThemeActionSet.Action());
    store.GetState<InspectorState>().Theme.ShouldBe(InspectorState.ThemeSystem);

    // Roots start open, nested paths start closed, diff paths start open, and the two never share a choice.
    await sender.Send(new InspectorState.ToggleNodeActionSet.Action("CounterState"));
    await sender.Send(new InspectorState.ToggleNodeActionSet.Action("CounterState.Items"));
    InspectorState state = store.GetState<InspectorState>();
    state.IsNodeExpanded("CounterState").ShouldBeFalse();
    state.IsNodeExpanded("CounterState.Items").ShouldBeTrue();
    state.IsDiffNodeExpanded("CounterState").ShouldBeTrue();

    await sender.Send(new InspectorState.ToggleDiffNodeActionSet.Action("CounterState"));
    state = store.GetState<InspectorState>();
    state.IsDiffNodeExpanded("CounterState").ShouldBeFalse();
    state.IsNodeExpanded("CounterState").ShouldBeFalse();

    await sender.Send(new InspectorState.ToggleDiffNodeActionSet.Action("CounterState"));
    await sender.Send(new InspectorState.ToggleNodeActionSet.Action(""));
    state = store.GetState<InspectorState>();
    state.IsDiffNodeExpanded("CounterState").ShouldBeTrue();
    state.ExpandedNodes.ContainsKey("").ShouldBeFalse();
  }

  private static ActionLogEntry Entry(string actionName, string storeId) =>
    new()
    {
      Name = "CounterState." + actionName,
      ActionName = actionName,
      StateTypeName = "CounterState",
      Status = "Ok",
      Sequence = 0,
      DurationMilliseconds = 1,
      StartTimeUtc = DateTime.UtcNow,
      SnapshotJson = """{"Count":1}""",
      StoreId = storeId
    };
}
