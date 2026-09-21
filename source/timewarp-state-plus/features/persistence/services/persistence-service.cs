#region Purpose
// Loads [PersistentState] snapshots from Blazored session/local storage using TimeWarp JSON options.
#endregion

#region Design
// Deserialize with TimeWarpStateOptions.JsonSerializerOptions so save (string write) and load share one contract.
// Key lookup is FullName then Name: new writes use FullName; leftover simple-name entries still load.
#endregion

namespace TimeWarp.Features.Persistence;

/// <summary>
/// Loads persisted state from browser storage.
/// </summary>
/// <remarks>
/// New writes (see <c>PersistentStatePostProcessor</c>) store under the state's <c>FullName</c>.
/// Load tries that key first, then the simple <c>Name</c> so existing session/local entries are not dropped.
/// JSON uses <see cref="TimeWarpStateOptions.JsonSerializerOptions"/>.
/// </remarks>
public class PersistenceService : IPersistenceService
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly ISessionStorageService SessionStorageService;
  private readonly ILocalStorageService LocalStorageService;
  private readonly ILogger<PersistenceService> Logger;
  private readonly ISender<ClientPipeline> Sender;

  public PersistenceService
  (
    ISender<ClientPipeline> sender,
    ISessionStorageService sessionStorageService,
    ILocalStorageService localStorageService,
    ILogger<PersistenceService> logger,
    TimeWarpStateOptions timeWarpStateOptions
  )
  {
    ArgumentNullException.ThrowIfNull(timeWarpStateOptions);
    Sender = sender;
    SessionStorageService = sessionStorageService;
    LocalStorageService = localStorageService;
    Logger = logger;
    JsonSerializerOptions = timeWarpStateOptions.JsonSerializerOptions;
  }

  public async Task<object?> LoadState(Type stateType, PersistentStateMethod persistentStateMethod)
  {
    string writeKey = PersistentStateStorageKey.ForWrite(stateType);

    Logger.LogInformation(EventIds.PersistenceService_LoadState, message: "Loading State for {stateType}", stateType);

    string? serializedState = await ReadSerializedState(writeKey, persistentStateMethod);
    if (string.IsNullOrEmpty(serializedState))
    {
      string nameKey = stateType.Name;
      if (!string.Equals(nameKey, writeKey, StringComparison.Ordinal))
      {
        serializedState = await ReadSerializedState(nameKey, persistentStateMethod);
      }
    }

    Logger.LogTrace
    (
      EventIds.PersistenceService_LoadState_SerializedState,
      message: "Serialized State: {serializedState}",
      serializedState
    );

    object? result = null;
    if (!string.IsNullOrEmpty(serializedState))
    {
      try
      {
        result = JsonSerializer.Deserialize(serializedState, stateType, JsonSerializerOptions);
      }
      catch (JsonException jsonException)
      {
        Logger.LogError
        (
          EventIds.PersistenceService_LoadState_DeserializationError,
          jsonException,
          message: "Error deserializing state for {stateType}",
          stateType
        );
        throw;
      }
    }

    if (result is IState state)
    {
      state.Sender = Sender;
    }

    return result;
  }

  private async Task<string?> ReadSerializedState(string storageKey, PersistentStateMethod persistentStateMethod)
  {
    return persistentStateMethod switch
    {
      PersistentStateMethod.SessionStorage => await SessionStorageService.GetItemAsStringAsync(storageKey),
      PersistentStateMethod.LocalStorage => await LocalStorageService.GetItemAsStringAsync(storageKey),
      PersistentStateMethod.PreRender => null, // TODO
      PersistentStateMethod.Server => null, // TODO
      _ => null
    };
  }
}
