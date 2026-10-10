#region Purpose
// Loads [PersistentState] snapshots from Blazored session/local storage using TimeWarp JSON options.
#endregion

#region Design
// Deserialize with a clone of TimeWarpStateOptions.JsonSerializerOptions (PropertyNameCaseInsensitive)
// so leftover Blazored PascalCase payloads under the simple Name key still bind; do not mutate the shared Store options.
// Key lookup is FullName then Name: new writes use FullName; leftover simple-name entries still load.
// Session and local storage services are optional. A missing service logs and loads nothing, matching
// PersistentStatePostProcessor. Register both with AddTimeWarpStatePersistence plus the Blazored helpers.
#endregion

namespace TimeWarp.Features.Persistence;

/// <summary>
/// Loads persisted state from browser storage.
/// </summary>
/// <remarks>
/// New writes (see <c>PersistentStatePostProcessor</c>) store under the state's <c>FullName</c>.
/// Load tries that key first, then the simple <c>Name</c> so existing session/local entries are not dropped.
/// JSON uses a case-insensitive clone of <see cref="TimeWarpStateOptions.JsonSerializerOptions"/>
/// so leftover Blazored PascalCase payloads under the simple Name key still bind.
/// </remarks>
public class PersistenceService : IPersistenceService
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly ISessionStorageService? SessionStorageService;
  private readonly ILocalStorageService? LocalStorageService;
  private readonly ILogger<PersistenceService> Logger;
  private readonly ISender<ClientPipeline> Sender;

  public PersistenceService
  (
    ISender<ClientPipeline> sender,
    ILogger<PersistenceService> logger,
    TimeWarpStateOptions timeWarpStateOptions,
    ISessionStorageService? sessionStorageService = null,
    ILocalStorageService? localStorageService = null
  )
  {
    ArgumentNullException.ThrowIfNull(timeWarpStateOptions);
    Sender = sender;
    SessionStorageService = sessionStorageService;
    LocalStorageService = localStorageService;
    Logger = logger;
    JsonSerializerOptions = new JsonSerializerOptions(timeWarpStateOptions.JsonSerializerOptions)
    {
      PropertyNameCaseInsensitive = true
    };
  }

  [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "stateType is a [PersistentState] state rooted by the generated StateCloneRegistry registration.")]
  [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "stateType is a [PersistentState] state rooted by the generated StateCloneRegistry registration.")]
  [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050", Justification = "stateType is a [PersistentState] state rooted by the generated StateCloneRegistry registration.")]
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
    switch (persistentStateMethod)
    {
      case PersistentStateMethod.SessionStorage when SessionStorageService is null:
        LogMissingStorage<ISessionStorageService>();
        return null;
      case PersistentStateMethod.SessionStorage:
        return await SessionStorageService!.GetItemAsStringAsync(storageKey);
      case PersistentStateMethod.LocalStorage when LocalStorageService is null:
        LogMissingStorage<ILocalStorageService>();
        return null;
      case PersistentStateMethod.LocalStorage:
        return await LocalStorageService!.GetItemAsStringAsync(storageKey);
      default:
        return null;
    }
  }

  private void LogMissingStorage<TService>() =>
    Logger.LogWarning
    (
      EventIds.PersistenceService_StorageNotRegistered,
      "No {ServiceName} is registered; skipping persistence load. Register it (for example AddBlazoredSessionStorage or AddBlazoredLocalStorage) in the host.",
      typeof(TService).Name
    );
}
