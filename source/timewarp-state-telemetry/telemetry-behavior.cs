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
// (no GetProperties); MaxSnapshotChars truncates the event payload only. The write itself is
// StateSnapshotRecorder, shared with the initialization handler so both use one cache key.
// Type names from typeof and DeclaringType; no AssemblyQualifiedName or Type.GetType.
// Per-closed-generic Type/Name are static readonly (finding 19). Snapshot failures never fail the action.
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

  private const string ActionTagName = StateSnapshotRecorder.ActionTagName;
  private const string StateTypeTagName = StateSnapshotRecorder.StateTypeTagName;

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
    if (EnclosingStateType is null)
    {
      return;
    }

    StateSnapshotRecorder.TryRecord
    (
      activity,
      Store,
      EnclosingStateType,
      ActionTypeName,
      TimeWarpStateTelemetryOptions,
      TimeWarpStateOptions,
      StateSnapshotCache,
      Logger
    );
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
