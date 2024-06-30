namespace TimeWarp.State.Plus;

using State.Extensions;

public class PersistentStatePostProcessor<TRequest, TResponse>
(
  ILogger<PersistentStatePostProcessor<TRequest, TResponse>> logger,
  IStore Store,
  ISessionStorageService SessionStorageService,
  ILocalStorageService LocalSessionStorageService
) : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : IAction
{
  private readonly ILogger Logger = logger;

  public async Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {

    Type currentType = typeof(TRequest).GetEnclosingStateType();
    
    PersistentStateAttribute? persistentStateAttribute =
      currentType.GetCustomAttribute<PersistentStateAttribute>();
      
    if (persistentStateAttribute is null) return;
    
    Logger.LogDebug("PersistentStatePostProcessor: {FullName}", typeof(TRequest).FullName);

    object state = Store.GetState(currentType);

    switch (persistentStateAttribute.PersistentStateMethod)
    {
      case PersistentStateMethod.Server:
        // TODO: 
        break;
      case PersistentStateMethod.SessionStorage:
        Logger.LogDebug("Save to Session Storage {StateTypeName}", currentType.Name);
        await SessionStorageService.SetItemAsync(currentType.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.LocalStorage:
        Logger.LogDebug("Save to Local Storage {StateTypeName}", currentType.Name);
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
