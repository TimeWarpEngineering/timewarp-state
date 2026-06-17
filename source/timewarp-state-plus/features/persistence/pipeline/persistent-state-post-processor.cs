namespace TimeWarp.State.Plus;

using TimeWarp.State.Extensions;
// Disambiguate from Microsoft.AspNetCore.Components.PersistentStateAttribute (added in .NET 10),
// which collides with TimeWarp's attribute under the global Components using.
using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;

public sealed class PersistentStatePostProcessor<TRequest, TResponse> : MessagePostProcessor<TRequest, TResponse>
  where TRequest : IAction
{
  private readonly ILogger Logger;
  private readonly IStore Store;
  private readonly ISessionStorageService SessionStorageService;
  private readonly ILocalStorageService LocalSessionStorageService;
  public PersistentStatePostProcessor
  (
    IStore store,
    ISessionStorageService sessionStorageService,
    ILocalStorageService localSessionStorageService,
    ILogger<PersistentStatePostProcessor<TRequest, TResponse>> logger
  )
  {
    Store = store;
    SessionStorageService = sessionStorageService;
    LocalSessionStorageService = localSessionStorageService;
    Logger = logger;
  }

  protected override async ValueTask Handle(TRequest request, TResponse response, CancellationToken cancellationToken)
  {

    Type currentType = typeof(TRequest).GetEnclosingStateType();
    
    PersistentStateAttribute? persistentStateAttribute =
      currentType.GetCustomAttribute<PersistentStateAttribute>();
      
    if (persistentStateAttribute is null) return;
    
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
        await LocalSessionStorageService.SetItemAsync(currentType.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.PreRender:
        // TODO: This needs to be tried and see if improves UX.
        break;
      default:
        throw new InvalidOperationException($"The {persistentStateAttribute.PersistentStateMethod} is not supported.");
    }
  }
}
