#region Purpose
// Emits one Activity per dispatched IAction with metadata; optional JSON snapshots as span events.
#endregion

#region Design
// Hot path: ActivitySource.HasListeners() then StartActivity. No GetState, no JSON, no snapshot
// cache when there is no listener or the span is unsampled.
// Default tags: nested DeclaringType action name, state type name, success/failure. Duration is
// the Activity. Snapshots only when IncludeSnapshots, IsAllDataRequested, and caller JsonTypeInfo
// exist. Serialize through JsonTypeInfo from caller options — never new JsonSerializerOptions()
// and never JsonSerializer.Serialize(object) on open TState. Diff is full-JSON string compare
// (no GetProperties); MaxSnapshotChars truncates the event payload only. Type names from typeof
// and DeclaringType; no AssemblyQualifiedName or Type.GetType. Per-closed-generic Type/Name are
// static readonly (finding 19). Snapshot failures never fail the action.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Pipeline behavior that records one OpenTelemetry <see cref="Activity"/> per TimeWarp.State action.
/// </summary>
/// <remarks>
/// Woven at compile time by <c>[assembly: MediatorBehavior]</c> into <see cref="ClientPipeline"/>.
/// Order 350 sits inside <c>StateTransactionBehavior</c> so handler failures are <c>Error</c> before
/// the transaction swallows them. Default spans carry nested type names, duration, and status only.
/// State JSON is a span event, and only when the host opts in and supplies <see cref="JsonTypeInfo"/>.
/// </remarks>
/// <typeparam name="TRequest">The action type.</typeparam>
/// <typeparam name="TResponse">The handler response type.</typeparam>
public sealed class TelemetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private static readonly Type ActionType = typeof(TRequest);
  private static readonly string ActionTypeName;
  private static readonly Type? EnclosingStateType;
  private static readonly string? EnclosingStateTypeName;
  private static readonly string ActivityName;

  private const string ActionTagName = "timewarp.state.action";
  private const string StateTypeTagName = "timewarp.state.state_type";
  private const string SnapshotJsonTagName = "snapshot.json";
  private const string SnapshotTruncatedTagName = "snapshot.truncated";
  private const string SnapshotEventName = "state.snapshot";
  private const string DiffEventName = "state.diff";

  private readonly ILogger Logger;
  private readonly IStore Store;
  private readonly TimeWarpStateTelemetryOptions? TimeWarpStateTelemetryOptions;
  private readonly TimeWarpStateOptions? TimeWarpStateOptions;
  private readonly StateSnapshotCache? StateSnapshotCache;

  static TelemetryBehavior()
  {
    if (typeof(TRequest).TryGetEnclosingStateType(out Type? enclosingStateType) && enclosingStateType is not null)
    {
      EnclosingStateType = enclosingStateType;
      EnclosingStateTypeName = enclosingStateType.Name;
    }

    ActivityName = NestedTypeName(ActionType, stopAtType: null);
    ActionTypeName = NestedTypeName(ActionType, stopAtType: EnclosingStateType);
  }

  /// <summary>
  /// Creates the behavior. Snapshot services are optional; metadata spans need only <see cref="IStore"/> for the type weave.
  /// </summary>
  public TelemetryBehavior
  (
    ILogger<TelemetryBehavior<TRequest, TResponse>> logger,
    IStore store,
    TimeWarpStateTelemetryOptions? timeWarpStateTelemetryOptions = null,
    TimeWarpStateOptions? timeWarpStateOptions = null,
    StateSnapshotCache? stateSnapshotCache = null
  )
  {
    Logger = logger;
    Store = store;
    TimeWarpStateTelemetryOptions = timeWarpStateTelemetryOptions;
    TimeWarpStateOptions = timeWarpStateOptions;
    StateSnapshotCache = stateSnapshotCache;

    Logger.LogDebug
    (
      EventIds.TelemetryBehavior_Constructing,
      "constructing {ClassName}<{RequestType},{ResponseType}>",
      nameof(TelemetryBehavior<TRequest, TResponse>),
      ActivityName,
      typeof(TResponse).Name
    );
  }

  /// <inheritdoc />
  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    if (!TimeWarpStateTelemetry.ActivitySource.HasListeners())
    {
      return await next(cancellationToken);
    }

    using Activity? activity = TimeWarpStateTelemetry.ActivitySource.StartActivity
    (
      ActivityName,
      ActivityKind.Internal
    );

    if (activity is null)
    {
      return await next(cancellationToken);
    }

    activity.SetTag(ActionTagName, ActionTypeName);
    if (EnclosingStateTypeName is not null)
    {
      activity.SetTag(StateTypeTagName, EnclosingStateTypeName);
    }

    try
    {
      TResponse response = await next(cancellationToken);
      activity.SetStatus(ActivityStatusCode.Ok);
      TryRecordSnapshot(activity);
      return response;
    }
    catch (Exception exception)
    {
      activity.SetStatus(ActivityStatusCode.Error, exception.Message);
      activity.AddException(exception);
      throw;
    }
  }

  private void TryRecordSnapshot(Activity activity)
  {
    if (!activity.IsAllDataRequested)
    {
      return;
    }

    if (TimeWarpStateTelemetryOptions is null || !TimeWarpStateTelemetryOptions.IncludeSnapshots)
    {
      return;
    }

    if (EnclosingStateType is null || StateSnapshotCache is null)
    {
      return;
    }

    JsonSerializerOptions? jsonSerializerOptions =
      TimeWarpStateTelemetryOptions.JsonSerializerOptions ?? TimeWarpStateOptions?.JsonSerializerOptions;

    if (jsonSerializerOptions is null)
    {
      Logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotSkipped,
        "Skipping snapshot for {ActionType}: no JsonSerializerOptions",
        ActionTypeName
      );
      return;
    }

    JsonTypeInfo? jsonTypeInfo = jsonSerializerOptions.TypeInfoResolver?.GetTypeInfo
    (
      EnclosingStateType,
      jsonSerializerOptions
    );

    if (jsonTypeInfo is null)
    {
      Logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotSkipped,
        "Skipping snapshot for {ActionType}: no JsonTypeInfo for {StateType}",
        ActionTypeName,
        EnclosingStateTypeName
      );
      return;
    }

    try
    {
      object state = Store.GetState(EnclosingStateType);
      string json = JsonSerializer.Serialize(state, jsonTypeInfo);

      string cacheKey = EnclosingStateType.FullName ?? EnclosingStateType.Name;
      SnapshotChange snapshotChange = StateSnapshotCache.Record(cacheKey, json);
      if (snapshotChange == SnapshotChange.Unchanged)
      {
        return;
      }

      string eventName = snapshotChange == SnapshotChange.Initial ? SnapshotEventName : DiffEventName;
      int maxSnapshotChars = TimeWarpStateTelemetryOptions.MaxSnapshotChars;
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
    catch (Exception exception)
    {
      Logger.LogDebug
      (
        EventIds.TelemetryBehavior_SnapshotFailed,
        exception,
        "Snapshot serialization failed for {ActionType}",
        ActionTypeName
      );
    }
  }

  private static string Truncate(string json, int maxSnapshotChars)
  {
    if (maxSnapshotChars <= 0 || json.Length <= maxSnapshotChars)
    {
      return json;
    }

    return string.Concat(json.AsSpan(0, maxSnapshotChars), "…(truncated)");
  }

  private static string NestedTypeName(Type type, Type? stopAtType)
  {
    List<string> names = [];
    Type? currentType = type;
    while (currentType is not null && currentType != stopAtType)
    {
      names.Add(currentType.Name);
      currentType = currentType.DeclaringType;
    }

    names.Reverse();
    return string.Join(".", names);
  }
}
