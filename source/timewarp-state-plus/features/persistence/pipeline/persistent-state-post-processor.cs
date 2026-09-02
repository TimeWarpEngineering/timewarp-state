namespace TimeWarp.State.Plus;

using TimeWarp.State.Extensions;
// Disambiguate from Microsoft.AspNetCore.Components.PersistentStateAttribute (added in .NET 10),
// which collides with TimeWarp's attribute under the global Components using.
using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;

/// <summary>
/// Pipeline behavior that saves a <c>[PersistentState]</c> state to its configured store after each of its actions.
/// Opt-in: the host declares <c>[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor&lt;,&gt;), order: ...)]</c>
/// and registers the Blazored storage services it uses. The behavior is woven at compile time for every
/// host of that assembly (including test hosts), so the storage services are optional dependencies: when
/// a <c>[PersistentState]</c> state is handled and its storage service is not registered, the save is
/// skipped with a warning instead of failing the action.
/// </summary>
public sealed class PersistentStatePostProcessor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger;
  private readonly IStore Store;
  private readonly ISessionStorageService? SessionStorageService;
  private readonly ILocalStorageService? LocalSessionStorageService;
  public PersistentStatePostProcessor
  (
    IStore store,
    ILogger<PersistentStatePostProcessor<TRequest, TResponse>> logger,
    ISessionStorageService? sessionStorageService = null,
    ILocalStorageService? localSessionStorageService = null
  )
  {
    Store = store;
    SessionStorageService = sessionStorageService;
    LocalSessionStorageService = localSessionStorageService;
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

    Type currentType = typeof(TRequest).GetEnclosingStateType();
    
    PersistentStateAttribute? persistentStateAttribute =
      currentType.GetCustomAttribute<PersistentStateAttribute>();
      
    if (persistentStateAttribute is null) return response;
    
    Logger.LogTrace(EventIds.PersistentStatePostProcessor_StartProcessing, "Start Processing: {FullName}", typeof(TRequest).FullName);

    object state = Store.GetState(currentType);

    switch (persistentStateAttribute.PersistentStateMethod)
    {
      case PersistentStateMethod.Server:
        // TODO: 
        break;
      case PersistentStateMethod.SessionStorage:
        Logger.LogTrace
        (
          EventIds.PersistentStatePostProcessor_SaveToSessionStorage
          ,"Save {StateTypeName} to Session Storage with value {json}"
          , currentType.Name
          , JsonSerializer.Serialize(state)
        );
        if (SessionStorageService is null)
        {
          LogMissingStorage<ISessionStorageService>(currentType);
          break;
        }
        await SessionStorageService.SetItemAsync(currentType.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.LocalStorage:
        Logger.LogTrace
        (
          EventIds.PersistentStatePostProcessor_SaveToLocalStorage
          ,"Save {StateTypeName} to Local Storage with value {json}"
          , currentType.Name
          , JsonSerializer.Serialize(state)
        );
        if (LocalSessionStorageService is null)
        {
          LogMissingStorage<ILocalStorageService>(currentType);
          break;
        }
        await LocalSessionStorageService.SetItemAsync(currentType.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.PreRender:
        // TODO: This needs to be tried and see if improves UX.
        break;
      default:
        throw new InvalidOperationException($"The {persistentStateAttribute.PersistentStateMethod} is not supported.");
    }

    return response;
  }

  private void LogMissingStorage<TService>(Type stateType) =>
    Logger.LogWarning
    (
      EventIds.PersistentStatePostProcessor_StorageNotRegistered,
      "{StateTypeName} is [PersistentState] but no {ServiceName} is registered; skipping persistence. Register it (e.g. AddBlazoredSessionStorage/AddBlazoredLocalStorage) in the host.",
      stateType.Name,
      typeof(TService).Name
    );
}
