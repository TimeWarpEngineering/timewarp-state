#region Purpose
// After each nested IAction, saves [PersistentState] snapshots to Blazored session or local storage.
#endregion

#region Design
// Enclosing state type and [PersistentState] are cached per closed generic so the hot path does not reflect on every IAction.
// Serialize with TimeWarpStateOptions.JsonSerializerOptions and write the JSON string (SetItemAsStringAsync) under FullName.
// Storage services stay optional: skip + warning when Blazored is not registered.
#endregion

namespace TimeWarp.State.Plus;

// Disambiguate from Microsoft.AspNetCore.Components.PersistentStateAttribute (added in .NET 10),
// which collides with TimeWarp's attribute under the global Components using.
using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;

/// <summary>
/// Pipeline behavior that saves a <c>[PersistentState]</c> state to its configured store after each of its actions.
/// Opt-in: the host declares <c>[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor&lt;,&gt;), order: ..., Scope = typeof(ClientPipeline))]</c>
/// and registers the Blazored storage services it uses. The behavior is woven at compile time for every
/// host of that assembly (including test hosts), so the storage services are optional dependencies: when
/// a <c>[PersistentState]</c> state is handled and its storage service is not registered, the save is
/// skipped with a warning instead of failing the action.
/// </summary>
/// <remarks>
/// New writes use the state's <c>FullName</c> as the storage key. Load (see <c>PersistenceService</c>)
/// tries FullName first, then the simple <c>Name</c>. JSON uses
/// <see cref="TimeWarpStateOptions.JsonSerializerOptions"/>.
/// </remarks>
public sealed class PersistentStatePostProcessor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private static readonly Type? EnclosingStateType;
  private static readonly PersistentStateAttribute? CachedPersistentStateAttribute;

  private readonly ILogger Logger;
  private readonly IStore Store;
  private readonly TimeWarpStateOptions TimeWarpStateOptions;
  private readonly ISessionStorageService? SessionStorageService;
  private readonly ILocalStorageService? LocalSessionStorageService;

  static PersistentStatePostProcessor()
  {
    if (!typeof(TRequest).TryGetEnclosingStateType(out Type? enclosingStateType) || enclosingStateType is null)
    {
      return;
    }

    EnclosingStateType = enclosingStateType;
    CachedPersistentStateAttribute = enclosingStateType.GetCustomAttribute<PersistentStateAttribute>();
  }

  public PersistentStatePostProcessor
  (
    IStore store,
    ILogger<PersistentStatePostProcessor<TRequest, TResponse>> logger,
    TimeWarpStateOptions timeWarpStateOptions,
    ISessionStorageService? sessionStorageService = null,
    ILocalStorageService? localSessionStorageService = null
  )
  {
    ArgumentNullException.ThrowIfNull(timeWarpStateOptions);
    Store = store;
    SessionStorageService = sessionStorageService;
    LocalSessionStorageService = localSessionStorageService;
    TimeWarpStateOptions = timeWarpStateOptions;
    Logger = logger;
  }

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    TResponse response = await next(cancellationToken);

    if (CachedPersistentStateAttribute is null || EnclosingStateType is null)
    {
      return response;
    }

    Logger.LogTrace(EventIds.PersistentStatePostProcessor_StartProcessing, "Start Processing: {FullName}", typeof(TRequest).FullName);

    object state = Store.GetState(EnclosingStateType);
    string storageKey = PersistentStateStorageKey.ForWrite(EnclosingStateType);
    string serializedState = SerializeState(state);

    switch (CachedPersistentStateAttribute.PersistentStateMethod)
    {
      case PersistentStateMethod.Server:
        // TODO:
        break;
      case PersistentStateMethod.SessionStorage:
        if (SessionStorageService is null)
        {
          LogMissingStorage<ISessionStorageService>(EnclosingStateType);
          break;
        }

        await WriteAsync
        (
          SessionStorageService.SetItemAsStringAsync,
          EventIds.PersistentStatePostProcessor_SaveToSessionStorage,
          "Session Storage",
          storageKey,
          serializedState,
          cancellationToken
        );
        break;
      case PersistentStateMethod.LocalStorage:
        if (LocalSessionStorageService is null)
        {
          LogMissingStorage<ILocalStorageService>(EnclosingStateType);
          break;
        }

        await WriteAsync
        (
          LocalSessionStorageService.SetItemAsStringAsync,
          EventIds.PersistentStatePostProcessor_SaveToLocalStorage,
          "Local Storage",
          storageKey,
          serializedState,
          cancellationToken
        );
        break;
      case PersistentStateMethod.PreRender:
        // TODO: This needs to be tried and see if improves UX.
        break;
      default:
        throw new InvalidOperationException($"The {CachedPersistentStateAttribute.PersistentStateMethod} is not supported.");
    }

    return response;
  }

  private string SerializeState(object state) =>
    JsonSerializer.Serialize(state, EnclosingStateType!, TimeWarpStateOptions.JsonSerializerOptions);

  private async Task WriteAsync
  (
    Func<string, string, CancellationToken, ValueTask> setItemAsStringAsync,
    EventId eventId,
    string storageKind,
    string storageKey,
    string serializedState,
    CancellationToken cancellationToken
  )
  {
    if (Logger.IsEnabled(LogLevel.Trace))
    {
      Logger.LogTrace
      (
        eventId,
        "Save {StateTypeName} to {StorageKind} with value {json}",
        EnclosingStateType!.FullName,
        storageKind,
        serializedState
      );
    }

    await setItemAsStringAsync(storageKey, serializedState, cancellationToken);
  }

  private void LogMissingStorage<TService>(Type stateType) =>
    Logger.LogWarning
    (
      EventIds.PersistentStatePostProcessor_StorageNotRegistered,
      "{StateTypeName} is [PersistentState] but no {ServiceName} is registered; skipping persistence. Register it (e.g. AddBlazoredSessionStorage/AddBlazoredLocalStorage) in the host.",
      stateType.FullName,
      typeof(TService).Name
    );
}
