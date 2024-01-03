#nullable enable
namespace BlazorState;

using BlazorState.Features.Persistence.Abstractions;
using BlazorState.Features.Persistence.Attributes;
using System.Threading.Tasks;

/// <summary>
/// 
/// </summary>
internal partial class Store : IStore
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly ILogger Logger;
  private readonly IServiceProvider ServiceProvider;
  private readonly IDictionary<string, IState> States;

  /// <summary>
  /// Unique Guid for the Store.
  /// </summary>
  /// <remarks>Useful when logging </remarks>
  public Guid Guid { get; } = Guid.NewGuid();

  public Store
  (
    ILogger<Store> aLogger,
    IServiceProvider aServiceProvider,
    BlazorStateOptions aBlazorStateOptions
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.Store_Initializing, "constructing with guid:{Guid}", Guid);
    ServiceProvider = aServiceProvider;
    JsonSerializerOptions = aBlazorStateOptions.JsonSerializerOptions;

    States = new Dictionary<string, IState>();
  }

  /// <summary>
  /// Get the State of the particular type
  /// </summary>
  /// <typeparam name="TState"></typeparam>
  /// <returns>The specific IState</returns>
  public async Task<TState> GetStateAsync<TState>()
  {
    Type stateType = typeof(TState);
    return (TState) (await GetStateAsync(stateType));
  }
  
  /// <summary>
  /// Clear all the states
  /// </summary>
  public void Reset() => States.Clear();

  /// <summary>
  /// Set the state for specific Type
  /// </summary>
  /// <param name="aNewState"></param>
  public void SetState(IState aNewState)
  {
    string typeName = aNewState.GetType().FullName;
    SetState(typeName, aNewState);
  }

  public async Task<object> GetStateAsync(Type type)
  {
    using (Logger.BeginScope(nameof(GetStateAsync)))
    {
      string typeName = type.FullName;

      if (!States.TryGetValue(typeName, out IState state))
      {
        Logger.LogDebug(EventIds.Store_CreateState, "Creating State of type: {typeName}", typeName);

        PersistentStateAttribute? persistentStateAttribute = type.GetCustomAttribute<PersistentStateAttribute>();
        if (persistentStateAttribute != null)
        {
          IPersistenceService persistenceService = ServiceProvider.GetRequiredService<IPersistenceService>();
          object? loadedState = await persistenceService.LoadStateAsync(type, persistentStateAttribute.PersistentStateMethod);
          state = loadedState as IState ?? (IState)ServiceProvider.GetRequiredService(type);
        }

        if (state == null)
        {
          state = (IState)ServiceProvider.GetRequiredService(type);
          state.Initialize();
        }

        States.Add(typeName, state);
      }
      else
      {
        Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, state.Guid);
      }
      return state;
    }
  }

  private void SetState(string aTypeName, object aNewState)
  {
    var newState = (IState)aNewState;
    Logger.LogDebug
    (
      EventIds.Store_SetState,
      "Assigning State. Type:{typeName}, Guid:{newState.Guid}",
      aTypeName,
      newState.Guid
    );
    States[aTypeName] = newState;
  }
}
