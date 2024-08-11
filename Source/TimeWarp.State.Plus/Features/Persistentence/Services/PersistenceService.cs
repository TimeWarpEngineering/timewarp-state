namespace TimeWarp.Features.Persistence;

public class PersistenceService : IPersistenceService
{
  private readonly JsonSerializerOptions JsonSerializerOptions = new();
  private readonly ISessionStorageService SessionStorageService;
  private readonly ILocalStorageService LocalStorageService;
  private readonly ILogger<PersistenceService> Logger;
  private readonly ISender Sender;
  public PersistenceService
  (
    ISender sender,
    ISessionStorageService sessionStorageService,
    ILocalStorageService localStorageService,
    ILogger<PersistenceService> logger
  )
  {
    Sender = sender;
    SessionStorageService = sessionStorageService;
    LocalStorageService = localStorageService;
    Logger = logger;
  }
  public async Task<object?> LoadState(Type stateType, PersistentStateMethod persistentStateMethod)
  {
    string typeName =
      stateType.Name ??
      throw new InvalidOperationException("The type provided has a null full name, which is not supported for persistence operations.");

    Logger.LogInformation(EventIds.PersistenceService_LoadState, "Loading State for {stateType}", stateType);

    string? serializedState = persistentStateMethod switch
    {
      PersistentStateMethod.SessionStorage => await SessionStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.LocalStorage => await LocalStorageService.GetItemAsStringAsync(typeName),
      PersistentStateMethod.PreRender => null, // TODO
      PersistentStateMethod.Server => null, // TODO
      _ => null
    };
    
    Logger.LogTrace
    (
      EventIds.PersistenceService_LoadState_SerializedState,
      "Serialized State: {serializedState}",
      serializedState
    );

    object? result = null;
    if (serializedState != null)
    {
      try
      {
        result = JsonSerializer.Deserialize(serializedState, stateType, JsonSerializerOptions);
      }
      catch (JsonException ex)
      {
        Logger.LogError(EventIds.PersistenceService_LoadState_DeserializationError, ex, "Error deserializing state for {stateType}", stateType);
        throw;
      }
    }

    if (result is IState state)
    {
      state.Sender = Sender;
    }

    return result;
  }
}
