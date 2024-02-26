namespace TimeWarp.State.Plus;

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

  public async Task Process(TRequest aRequest, TResponse response, CancellationToken cancellationToken)
  {
    Logger.LogDebug("PersistentStatePostProcessor: {FullName}", typeof(TRequest).FullName);

    Type currentType = typeof(TRequest);
    while (currentType.DeclaringType != null)
    {
      currentType = currentType.DeclaringType;
    }
    // currentType is now the top-level non-nested type

    PersistentStateAttribute? persistentStateAttribute =
      currentType.GetCustomAttribute<PersistentStateAttribute>();
      
    if (persistentStateAttribute is null) return;

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
