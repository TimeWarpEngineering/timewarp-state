namespace TimeWarpState.Middleware.PersistentState;

public class PersistenceService
(
  ISessionStorageService sessionStorageService,
  ILocalStorageService localStorageService,
  JsonSerializerOptions jsonSerializerOptions
) : IPersistenceService
{
  private readonly ISessionStorageService SessionStorageService = sessionStorageService;
  private readonly ILocalStorageService LocalStorageService = localStorageService;
  private readonly JsonSerializerOptions JsonSerializerOptions = jsonSerializerOptions;

  public async Task<object?> LoadStateAsync(Type stateType, PersistentStateMethod persistentStateMethod)
  {
    string typeName = 
      stateType.FullName ?? 
      throw new InvalidOperationException("The type provided has a null full name, which is not supported for persistence operations.");

    string? serializedState = persistentStateMethod switch
    {
      PersistentStateMethod.SessionStorage => await SessionStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.LocalStorage => await LocalStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.Server => null,// TODO
      _ => null,
    };

    return 
      serializedState != null ?
      JsonSerializer.Deserialize(serializedState, stateType, JsonSerializerOptions) :
      null;
  }
}
