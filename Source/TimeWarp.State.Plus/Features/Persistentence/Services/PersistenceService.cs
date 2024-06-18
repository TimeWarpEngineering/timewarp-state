namespace TimeWarp.Features.Persistence;

public class PersistenceService : IPersistenceService
{
  private readonly JsonSerializerOptions JsonSerializerOptions = new();
  private readonly ISessionStorageService SessionStorageService;
  private readonly ILocalStorageService LocalStorageService;
  private readonly ILogger<PersistenceService> Logger;
  public PersistenceService
  (
    ISessionStorageService sessionStorageService,
    ILocalStorageService localStorageService,
    ILogger<PersistenceService> logger)
  {
    SessionStorageService = sessionStorageService;
    LocalStorageService = localStorageService;
    Logger = logger;
  }
  public async Task<object?> LoadState(Type stateType, PersistentStateMethod persistentStateMethod)
  {
    string typeName = 
      stateType.Name ?? 
      throw new InvalidOperationException("The type provided has a null full name, which is not supported for persistence operations.");
    
    Logger.LogInformation("PersistenceService.LoadState Loading State for {stateType}", stateType);

    string? serializedState = persistentStateMethod switch
    {
      PersistentStateMethod.SessionStorage => await SessionStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.LocalStorage => await LocalStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.PreRender => null,// TODO
      PersistentStateMethod.Server => null,// TODO
      _ => null
    };
    
    object? result = 
      serializedState != null ?
      JsonSerializer.Deserialize(serializedState, stateType, JsonSerializerOptions) :
      null;

    return result;
  }
}
