namespace TimeWarp.State.Plus.PersistentState;

public class PersistenceService
(
  ISessionStorageService SessionStorageService,
  ILocalStorageService LocalStorageService,
  ILogger<PersistenceService> Logger
) : IPersistenceService
{
  private readonly JsonSerializerOptions JsonSerializerOptions = new();
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
