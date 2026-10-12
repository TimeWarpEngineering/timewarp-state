#region Purpose
// Proves an initialized state emits state.snapshot when snapshots are on, and that the next change is a diff.
#endregion

#region Design
// Constructs InitializedStateSnapshotHandler beside TelemetryBehavior on one cache and RecordingStore.
// Skip cases assert GetState stays at zero: no listener, IncludeSnapshots off, or no JsonTypeInfo.
#endregion

namespace TimeWarp.State.Telemetry.Tests;

public class InitializedStateSnapshotHandler_Should
{
  public async Task Emit_Init_Snapshot_Then_Diff_On_The_Next_Change()
  {
    TelemetryTestState telemetryTestState = new() { Count = 1 };
    RecordingStore recordingStore = new(telemetryTestState);
    StateSnapshotCache stateSnapshotCache = new();
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = SnapshotOptions();
    InitializedStateSnapshotHandler handler = Handler(recordingStore, timeWarpStateTelemetryOptions, stateSnapshotCache);
    TelemetryBehavior<TelemetryTestState.IncrementAction, Unit> incrementBehavior = Behavior
    (
      recordingStore,
      timeWarpStateTelemetryOptions,
      stateSnapshotCache
    );

    using ActivityListenerHarness activityListenerHarness = new();
    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    telemetryTestState.Count = 2;
    await incrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    activityListenerHarness.Activities.Count.ShouldBe(2);
    RecordedActivity initialized = activityListenerHarness.Activities[0];
    initialized.DisplayName.ShouldBe("TelemetryTestState.Initialized");
    initialized.Status.ShouldBe(ActivityStatusCode.Ok);
    TagValue(initialized, "timewarp.state.action").ShouldBe("init");
    TagValue(initialized, "timewarp.state.state_type").ShouldBe("TelemetryTestState");
    TagValue(initialized, "timewarp.state.kind").ShouldBe("init");
    TagValue(initialized, "timewarp.state.store").ShouldBe(recordingStore.Guid.ToString("D"));
    ActivityEvent snapshotEvent = initialized.Events.ShouldHaveSingleItem();
    snapshotEvent.Name.ShouldBe("state.snapshot");
    EventJson(snapshotEvent).ShouldContain("\"count\":1");

    ActivityEvent diffEvent = activityListenerHarness.Activities[1].Events.ShouldHaveSingleItem();
    diffEvent.Name.ShouldBe("state.diff");
    EventJson(diffEvent).ShouldContain("\"count\":2");
    recordingStore.GetStateCallCount.ShouldBe(2);
  }

  public async Task Tag_Init_With_The_Render_Kind_When_The_Host_Supplies_One()
  {
    RecordingStore recordingStore = new(new TelemetryTestState { Count = 1 });
    InitializedStateSnapshotHandler handler = new
    (
      NullLogger<InitializedStateSnapshotHandler>.Instance,
      recordingStore,
      SnapshotOptions(),
      timeWarpStateOptions: null,
      new StateSnapshotCache(),
      new FixedRenderKind(StateRenderKinds.Prerender)
    );

    using ActivityListenerHarness activityListenerHarness = new();
    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    RecordedActivity initialized = activityListenerHarness.Activities.ShouldHaveSingleItem();
    TagValue(initialized, "timewarp.state.store").ShouldBe(recordingStore.Guid.ToString("D"));
    TagValue(initialized, "timewarp.state.render").ShouldBe(StateRenderKinds.Prerender);
    TagValue(initialized, "timewarp.state.kind").ShouldBe("init");
  }

  public async Task Truncate_The_Init_Snapshot_Past_MaxSnapshotChars()
  {
    RecordingStore recordingStore = new(new TelemetryTestState { Count = 1 });
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = SnapshotOptions();
    timeWarpStateTelemetryOptions.MaxSnapshotChars = 8;
    InitializedStateSnapshotHandler handler = Handler(recordingStore, timeWarpStateTelemetryOptions, new StateSnapshotCache());

    using ActivityListenerHarness activityListenerHarness = new();
    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    RecordedActivity initialized = activityListenerHarness.Activities.ShouldHaveSingleItem();
    initialized.Status.ShouldBe(ActivityStatusCode.Ok);
    ActivityEvent snapshotEvent = initialized.Events.ShouldHaveSingleItem();
    snapshotEvent.Name.ShouldBe("state.snapshot");
    EventJson(snapshotEvent).ShouldContain("…(truncated)");
    snapshotEvent.Tags.Single(tag => tag.Key == "snapshot.truncated").Value.ShouldBe(true);
  }

  public async Task Set_Error_Status_When_Serialization_Fails()
  {
    RecordingStore recordingStore = new(new TelemetryTestState { Count = 1 }) { ThrowOnGetState = true };
    InitializedStateSnapshotHandler handler = Handler(recordingStore, SnapshotOptions(), new StateSnapshotCache());

    using ActivityListenerHarness activityListenerHarness = new();
    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    RecordedActivity initialized = activityListenerHarness.Activities.ShouldHaveSingleItem();
    initialized.Status.ShouldBe(ActivityStatusCode.Error);
    initialized.Events.ShouldBeEmpty();
  }

  public async Task Skip_When_IncludeSnapshots_Is_False()
  {
    RecordingStore recordingStore = new(new TelemetryTestState { Count = 0 });
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = SnapshotOptions();
    timeWarpStateTelemetryOptions.IncludeSnapshots = false;
    InitializedStateSnapshotHandler handler = Handler(recordingStore, timeWarpStateTelemetryOptions, new StateSnapshotCache());

    using ActivityListenerHarness activityListenerHarness = new();
    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    activityListenerHarness.Activities.ShouldBeEmpty();
    recordingStore.GetStateCallCount.ShouldBe(0);
  }

  public async Task Skip_When_The_Resolver_Returns_No_Type_Info()
  {
    RecordingStore recordingStore = new(new TelemetryTestState { Count = 0 });
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = new()
    {
      IncludeSnapshots = true,
      JsonSerializerOptions = new JsonSerializerOptions
      {
        TypeInfoResolver = new EmptyJsonTypeInfoResolver()
      }
    };
    InitializedStateSnapshotHandler handler = Handler(recordingStore, timeWarpStateTelemetryOptions, new StateSnapshotCache());

    using ActivityListenerHarness activityListenerHarness = new();
    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    activityListenerHarness.Activities.ShouldBeEmpty();
    recordingStore.GetStateCallCount.ShouldBe(0);
  }

  public async Task Skip_When_No_Listener_Is_Attached()
  {
    RecordingStore recordingStore = new(new TelemetryTestState { Count = 0 });
    InitializedStateSnapshotHandler handler = Handler(recordingStore, SnapshotOptions(), new StateSnapshotCache());

    await handler.Handle(new StateInitializedNotification(typeof(TelemetryTestState)), CancellationToken.None);

    recordingStore.GetStateCallCount.ShouldBe(0);
  }

  private static TimeWarpStateTelemetryOptions SnapshotOptions() =>
    new()
    {
      IncludeSnapshots = true,
      JsonSerializerOptions = new JsonSerializerOptions
      {
        TypeInfoResolver = TelemetryTestJsonContext.Default
      }
    };

  private static InitializedStateSnapshotHandler Handler
  (
    RecordingStore recordingStore,
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions,
    StateSnapshotCache stateSnapshotCache
  ) =>
    new
    (
      NullLogger<InitializedStateSnapshotHandler>.Instance,
      recordingStore,
      timeWarpStateTelemetryOptions,
      timeWarpStateOptions: null,
      stateSnapshotCache
    );

  private static TelemetryBehavior<TelemetryTestState.IncrementAction, Unit> Behavior
  (
    RecordingStore recordingStore,
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions,
    StateSnapshotCache stateSnapshotCache
  ) =>
    new
    (
      NullLogger<TelemetryBehavior<TelemetryTestState.IncrementAction, Unit>>.Instance,
      recordingStore,
      timeWarpStateTelemetryOptions,
      timeWarpStateOptions: null,
      stateSnapshotCache
    );

  private static object? TagValue(RecordedActivity recordedActivity, string key) =>
    recordedActivity.Tags.Single(tag => tag.Key == key).Value;

  private static string EventJson(ActivityEvent activityEvent) =>
    activityEvent.Tags.Single(tag => tag.Key == "snapshot.json").Value.ShouldBeOfType<string>();

  private sealed class FixedRenderKind(string name) : IStateRenderKind
  {
    public string? Name { get; } = name;

    public void Observe(bool isInteractive) { }
  }

  private sealed class EmptyJsonTypeInfoResolver : IJsonTypeInfoResolver
  {
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) => null;
  }
}
