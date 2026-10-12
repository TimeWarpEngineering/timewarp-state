#region Purpose
// Emits a state.snapshot activity when a state finishes Initialize.
#endregion

#region Design
// StateInitializedNotification is published from Store.GetState after the instance is inserted and
// Initialize has returned. This handler does not await, so the snapshot is recorded before GetState
// returns and before a later action span runs. The cache key matches TelemetryBehavior, so the first
// mutating action is a state.diff. IncludeSnapshots off, no listener, or no JsonTypeInfo skips the
// activity and logs the same warning as an action snapshot. A sampled-out init span (no activity or
// IsAllDataRequested false) does not prime the cache, so the first action then emits state.snapshot
// rather than state.diff. The span carries the store Guid, and the render kind when the host
// recorded one. No property reflection.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Publishes an initialization span for a state when snapshots are opted in.
/// </summary>
public sealed class InitializedStateSnapshotHandler : INotificationHandler<StateInitializedNotification>
{
  private readonly ILogger Logger;
  private readonly IStore Store;
  private readonly TimeWarpStateTelemetryOptions? TimeWarpStateTelemetryOptions;
  private readonly TimeWarpStateOptions? TimeWarpStateOptions;
  private readonly StateSnapshotCache? StateSnapshotCache;
  private readonly IStateRenderKind? StateRenderKind;

  /// <summary>
  /// Creates the handler. Snapshot services are optional; without them the handler returns.
  /// </summary>
  public InitializedStateSnapshotHandler
  (
    ILogger<InitializedStateSnapshotHandler> logger,
    IStore store,
    TimeWarpStateTelemetryOptions? timeWarpStateTelemetryOptions = null,
    TimeWarpStateOptions? timeWarpStateOptions = null,
    StateSnapshotCache? stateSnapshotCache = null,
    IStateRenderKind? stateRenderKind = null
  )
  {
    Logger = logger;
    Store = store;
    TimeWarpStateTelemetryOptions = timeWarpStateTelemetryOptions;
    TimeWarpStateOptions = timeWarpStateOptions;
    StateSnapshotCache = stateSnapshotCache;
    StateRenderKind = stateRenderKind;
  }

  /// <inheritdoc />
  public Task Handle(StateInitializedNotification notification, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(notification);
    try
    {
      Record(notification.StateType);
    }
    catch (Exception exception)
    {
      Logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotFailed,
        exception,
        "Initialization snapshot failed for {StateType}",
        notification.StateType.Name
      );
    }

    return Task.CompletedTask;
  }

  private void Record(Type stateType)
  {
    if (!TimeWarpStateTelemetry.ActivitySource.HasListeners())
    {
      return;
    }

    if (TimeWarpStateTelemetryOptions is null || !TimeWarpStateTelemetryOptions.IncludeSnapshots)
    {
      return;
    }

    if (StateSnapshotCache is null)
    {
      return;
    }

    string stateTypeName = stateType.Name;
    string logName = stateTypeName + ".Initialized";
    JsonSerializerOptions? jsonSerializerOptions = StateSnapshotRecorder.ResolveOptions
    (
      TimeWarpStateTelemetryOptions,
      TimeWarpStateOptions
    );
    if (jsonSerializerOptions is null)
    {
      Logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotSkipped,
        "Skipping snapshot for {ActionType}: no JsonSerializerOptions",
        logName
      );
      return;
    }

    if (StateSnapshotRecorder.ResolveTypeInfo(stateType, jsonSerializerOptions) is null)
    {
      Logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotSkipped,
        "Skipping snapshot for {ActionType}: no JsonTypeInfo for {StateType}",
        logName,
        stateTypeName
      );
      return;
    }

    using Activity? activity = TimeWarpStateTelemetry.ActivitySource.StartActivity
    (
      logName,
      ActivityKind.Internal
    );
    if (activity is null || !activity.IsAllDataRequested)
    {
      return;
    }

    activity.SetTag(StateSnapshotRecorder.ActionTagName, StateSnapshotRecorder.InitKind);
    activity.SetTag(StateSnapshotRecorder.StateTypeTagName, stateTypeName);
    activity.SetTag(StateSnapshotRecorder.KindTagName, StateSnapshotRecorder.InitKind);
    StateSnapshotRecorder.TagOrigin(activity, Store, StateRenderKind);

    if
    (
      !StateSnapshotRecorder.TryCaptureJson
      (
        Store,
        stateType,
        logName,
        TimeWarpStateTelemetryOptions,
        TimeWarpStateOptions,
        Logger,
        out string json
      )
    )
    {
      activity.SetStatus(ActivityStatusCode.Error, "snapshot failed");
      return;
    }

    string cacheKey = stateType.FullName ?? stateType.Name;
    StateSnapshotCache.Record(cacheKey, json);
    StateSnapshotRecorder.AddEvent
    (
      activity,
      StateSnapshotRecorder.SnapshotEventName,
      json,
      TimeWarpStateTelemetryOptions.MaxSnapshotChars
    );
    activity.SetStatus(ActivityStatusCode.Ok);
  }
}
