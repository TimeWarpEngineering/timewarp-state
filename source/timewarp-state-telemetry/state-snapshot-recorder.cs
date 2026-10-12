#region Purpose
// Shared snapshot serialize, cache, and span-event write for action spans and initialization spans.
#endregion

#region Design
// One JSON path so action diffs and initialization snapshots share the cache key (Type.FullName),
// the truncation rule, and the JsonTypeInfo lookup. Callers decide whether a missing type info
// skips the activity. This type never constructs JsonSerializerOptions and never reflects properties.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Writes opt-in state JSON onto an <see cref="Activity"/>.
/// </summary>
internal static class StateSnapshotRecorder
{
  internal const string ActionTagName = "timewarp.state.action";
  internal const string StateTypeTagName = "timewarp.state.state_type";
  internal const string KindTagName = "timewarp.state.kind";
  internal const string InitKind = "init";
  internal const string SnapshotJsonTagName = "snapshot.json";
  internal const string SnapshotTruncatedTagName = "snapshot.truncated";
  internal const string SnapshotEventName = "state.snapshot";
  internal const string DiffEventName = "state.diff";

  internal static JsonSerializerOptions? ResolveOptions
  (
    TimeWarpStateTelemetryOptions? telemetryOptions,
    TimeWarpStateOptions? timeWarpStateOptions
  ) =>
    telemetryOptions?.JsonSerializerOptions ?? timeWarpStateOptions?.JsonSerializerOptions;

  internal static JsonTypeInfo? ResolveTypeInfo(Type stateType, JsonSerializerOptions? jsonSerializerOptions) =>
    jsonSerializerOptions?.TypeInfoResolver?.GetTypeInfo(stateType, jsonSerializerOptions);

  /// <summary>
  /// Serializes <paramref name="stateType"/> and, when the JSON changed, adds a snapshot or diff event.
  /// Returns false when no event was added. Does not throw.
  /// </summary>
  internal static bool TryRecord
  (
    Activity activity,
    IStore store,
    Type stateType,
    string logName,
    TimeWarpStateTelemetryOptions? telemetryOptions,
    TimeWarpStateOptions? timeWarpStateOptions,
    StateSnapshotCache? cache,
    ILogger logger
  )
  {
    if (!activity.IsAllDataRequested)
    {
      return false;
    }

    if (telemetryOptions is null || !telemetryOptions.IncludeSnapshots || cache is null)
    {
      return false;
    }

    if (!TryCaptureJson(store, stateType, logName, telemetryOptions, timeWarpStateOptions, logger, out string json))
    {
      return false;
    }

    string cacheKey = stateType.FullName ?? stateType.Name;
    SnapshotChange snapshotChange = cache.Record(cacheKey, json);
    if (snapshotChange == SnapshotChange.Unchanged)
    {
      return false;
    }

    string eventName = snapshotChange == SnapshotChange.Initial ? SnapshotEventName : DiffEventName;
    AddEvent(activity, eventName, json, telemetryOptions.MaxSnapshotChars);
    return true;
  }

  internal static bool TryCaptureJson
  (
    IStore store,
    Type stateType,
    string logName,
    TimeWarpStateTelemetryOptions telemetryOptions,
    TimeWarpStateOptions? timeWarpStateOptions,
    ILogger logger,
    out string json
  )
  {
    json = "";
    JsonSerializerOptions? jsonSerializerOptions = ResolveOptions(telemetryOptions, timeWarpStateOptions);
    if (jsonSerializerOptions is null)
    {
      logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotSkipped,
        "Skipping snapshot for {ActionType}: no JsonSerializerOptions",
        logName
      );
      return false;
    }

    JsonTypeInfo? jsonTypeInfo = ResolveTypeInfo(stateType, jsonSerializerOptions);
    if (jsonTypeInfo is null)
    {
      logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotSkipped,
        "Skipping snapshot for {ActionType}: no JsonTypeInfo for {StateType}",
        logName,
        stateType.Name
      );
      return false;
    }

    try
    {
      object state = store.GetState(stateType);
      json = JsonSerializer.Serialize(state, jsonTypeInfo);
      return true;
    }
    catch (Exception exception)
    {
      logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotFailed,
        exception,
        "Snapshot serialization failed for {ActionType}",
        logName
      );
      return false;
    }
  }

  internal static void AddEvent(Activity activity, string eventName, string json, int maxSnapshotChars)
  {
    bool snapshotTruncated = maxSnapshotChars > 0 && json.Length > maxSnapshotChars;
    activity.AddEvent
    (
      new ActivityEvent
      (
        eventName,
        tags: new ActivityTagsCollection
        {
          { SnapshotJsonTagName, Truncate(json, maxSnapshotChars) },
          { SnapshotTruncatedTagName, snapshotTruncated }
        }
      )
    );
  }

  private static string Truncate(string json, int maxSnapshotChars)
  {
    if (maxSnapshotChars <= 0 || json.Length <= maxSnapshotChars)
    {
      return json;
    }

    return string.Concat(json.AsSpan(0, maxSnapshotChars), "…(truncated)");
  }
}
