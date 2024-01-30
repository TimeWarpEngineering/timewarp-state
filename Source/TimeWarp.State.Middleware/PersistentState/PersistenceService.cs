namespace TimeWarp.State.Middleware.PersistentState;

using Microsoft.Extensions.Logging;

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

    // TODO: Remove these comments potentially add Debug Logging.
    Logger.LogInformation($"PersistenceService.LoadState Loading State for {stateType}");

    string? serializedState = persistentStateMethod switch
    {
      PersistentStateMethod.SessionStorage => await SessionStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.LocalStorage => await LocalStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.Server => null,// TODO
      _ => null,
    };
    
    Console.WriteLine($"PersistenceService.LoadState serializedState: {serializedState}");

    object? result = 
      serializedState != null ?
      JsonSerializer.Deserialize(serializedState, stateType, JsonSerializerOptions) :
      null;

    Console.WriteLine($"PersistenceService.LoadState serializedState: {serializedState}");

    return result;
  }
}
