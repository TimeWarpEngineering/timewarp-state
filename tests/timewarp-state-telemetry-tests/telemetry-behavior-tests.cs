#region Purpose
// Proves TelemetryBehavior emits activities, sets error status, skips work with no listener,
// and records opt-in snapshots only through caller JsonTypeInfo.
#endregion

namespace TelemetryBehaviorTests;

public class Should_
{
  public async Task Invoke_Next_Without_GetState_When_No_Listener_Is_Attached()
  {
    Harness harness = CreateHarness();
    bool nextCalled = false;

    Unit result = await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ =>
      {
        nextCalled = true;
        return Task.FromResult(Unit.Value);
      },
      CancellationToken.None
    );

    nextCalled.ShouldBeTrue();
    result.ShouldBe(Unit.Value);
    harness.Store.GetStateCallCount.ShouldBe(0);
  }

  public async Task Emit_Activity_With_Action_And_State_Tags_On_Success()
  {
    Harness harness = CreateHarness();
    using ActivityListenerHarness activityListenerHarness = new();

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    activityListenerHarness.Activities.Count.ShouldBe(1);
    RecordedActivity recordedActivity = activityListenerHarness.Activities[0];
    recordedActivity.DisplayName.ShouldBe("TelemetryTestState.IncrementAction");
    recordedActivity.Status.ShouldBe(ActivityStatusCode.Ok);
    TagValue(recordedActivity, "timewarp.state.action").ShouldBe("IncrementAction");
    TagValue(recordedActivity, "timewarp.state.state_type").ShouldBe("TelemetryTestState");
    recordedActivity.Events.ShouldBeEmpty();
    harness.Store.GetStateCallCount.ShouldBe(0);
  }

  public async Task Set_Error_Status_And_Rethrow_When_Handler_Throws()
  {
    Harness harness = CreateHarness();
    using ActivityListenerHarness activityListenerHarness = new();
    InvalidOperationException thrown = new("handler failed");

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      () => harness.ThrowBehavior.Handle
      (
        new TelemetryTestState.ThrowAction(),
        _ => Task.FromException<Unit>(thrown),
        CancellationToken.None
      )
    );

    exception.ShouldBeSameAs(thrown);
    activityListenerHarness.Activities.Count.ShouldBe(1);
    RecordedActivity recordedActivity = activityListenerHarness.Activities[0];
    recordedActivity.DisplayName.ShouldBe("TelemetryTestState.ThrowAction");
    recordedActivity.Status.ShouldBe(ActivityStatusCode.Error);
    recordedActivity.StatusDescription.ShouldBe("handler failed");
    recordedActivity.HasExceptionEvent.ShouldBeTrue();
    harness.Store.GetStateCallCount.ShouldBe(0);
  }

  public async Task Skip_GetState_When_Listener_Is_Attached_But_Span_Is_Not_Sampled()
  {
    Harness harness = CreateHarness(includeSnapshots: true);
    using ActivityListenerHarness activityListenerHarness = new(ActivitySamplingResult.None);

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    activityListenerHarness.Activities.ShouldBeEmpty();
    harness.Store.GetStateCallCount.ShouldBe(0);
  }

  public async Task Emit_Snapshot_Then_Diff_Through_Caller_JsonTypeInfo()
  {
    Harness harness = CreateHarness(includeSnapshots: true);
    using ActivityListenerHarness activityListenerHarness = new();

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    harness.Store.CurrentState.ShouldBeOfType<TelemetryTestState>().Count = 2;

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    activityListenerHarness.Activities.Count.ShouldBe(3);
    harness.Store.GetStateCallCount.ShouldBe(3);

    ActivityEvent snapshotEvent = activityListenerHarness.Activities[0].Events.ShouldHaveSingleItem();
    snapshotEvent.Name.ShouldBe("state.snapshot");
    EventJson(snapshotEvent).ShouldContain("\"count\":1");

    ActivityEvent diffEvent = activityListenerHarness.Activities[1].Events.ShouldHaveSingleItem();
    diffEvent.Name.ShouldBe("state.diff");
    EventJson(diffEvent).ShouldContain("\"count\":2");

    activityListenerHarness.Activities[2].Events.ShouldBeEmpty();
  }

  public async Task Skip_Snapshot_When_IncludeSnapshots_Is_False()
  {
    Harness harness = CreateHarness(includeSnapshots: false);
    using ActivityListenerHarness activityListenerHarness = new();

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    activityListenerHarness.Activities.ShouldHaveSingleItem().Events.ShouldBeEmpty();
    harness.Store.GetStateCallCount.ShouldBe(0);
  }

  public async Task Skip_Snapshot_When_TypeInfoResolver_Cannot_Describe_The_State()
  {
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = new()
    {
      IncludeSnapshots = true,
      JsonSerializerOptions = new JsonSerializerOptions
      {
        TypeInfoResolver = new EmptyJsonTypeInfoResolver()
      }
    };

    Harness harness = CreateHarness(timeWarpStateTelemetryOptions);
    using ActivityListenerHarness activityListenerHarness = new();

    await harness.IncrementBehavior.Handle
    (
      new TelemetryTestState.IncrementAction(),
      _ => Task.FromResult(Unit.Value),
      CancellationToken.None
    );

    activityListenerHarness.Activities.ShouldHaveSingleItem().Events.ShouldBeEmpty();
    harness.Store.GetStateCallCount.ShouldBe(0);
  }

  private static Harness CreateHarness(bool includeSnapshots = false)
  {
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = new()
    {
      IncludeSnapshots = includeSnapshots,
      JsonSerializerOptions = new JsonSerializerOptions
      {
        TypeInfoResolver = TelemetryTestJsonContext.Default
      }
    };

    return CreateHarness(timeWarpStateTelemetryOptions);
  }

  private static Harness CreateHarness(TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions)
  {
    TelemetryTestState telemetryTestState = new() { Count = 1 };
    RecordingStore recordingStore = new(telemetryTestState);
    StateSnapshotCache stateSnapshotCache = new();

    return new Harness
    {
      Store = recordingStore,
      IncrementBehavior = new TelemetryBehavior<TelemetryTestState.IncrementAction, Unit>
      (
        NullLogger<TelemetryBehavior<TelemetryTestState.IncrementAction, Unit>>.Instance,
        recordingStore,
        timeWarpStateTelemetryOptions,
        timeWarpStateOptions: null,
        stateSnapshotCache
      ),
      ThrowBehavior = new TelemetryBehavior<TelemetryTestState.ThrowAction, Unit>
      (
        NullLogger<TelemetryBehavior<TelemetryTestState.ThrowAction, Unit>>.Instance,
        recordingStore,
        timeWarpStateTelemetryOptions,
        timeWarpStateOptions: null,
        stateSnapshotCache
      )
    };
  }

  private static object? TagValue(RecordedActivity recordedActivity, string key) =>
    recordedActivity.Tags.Single(tag => tag.Key == key).Value;

  private static string EventJson(ActivityEvent activityEvent) =>
    activityEvent.Tags.Single(tag => tag.Key == "snapshot.json").Value.ShouldBeOfType<string>();

  private sealed class Harness
  {
    public required RecordingStore Store { get; init; }
    public required TelemetryBehavior<TelemetryTestState.IncrementAction, Unit> IncrementBehavior { get; init; }
    public required TelemetryBehavior<TelemetryTestState.ThrowAction, Unit> ThrowBehavior { get; init; }
  }

  private sealed class EmptyJsonTypeInfoResolver : IJsonTypeInfoResolver
  {
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) => null;
  }
}
