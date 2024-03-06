namespace BlazorState;

/// <summary>
/// 
/// </summary>
internal partial class Store : IStore
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly ILogger Logger;
  private readonly IServiceProvider ServiceProvider;
  private readonly IDictionary<string, IState> States;
  private readonly IPublisher Publisher;
  private readonly BlazorStateOptions BlazorStateOptions;

  /// <summary>
  /// Unique Guid for the Store.
  /// </summary>
  /// <remarks>Useful when logging </remarks>
  public Guid Guid { get; } = Guid.NewGuid();

  public Store
  (
    ILogger<Store> logger,
    IServiceProvider serviceProvider,
    BlazorStateOptions blazorStateOptions,
    IPublisher publisher
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.Store_Initializing, "constructing with guid:{Guid}", Guid);
    ServiceProvider = serviceProvider;
    Publisher = publisher;
    BlazorStateOptions = blazorStateOptions;
    JsonSerializerOptions = blazorStateOptions.JsonSerializerOptions;

    States = new ConcurrentDictionary<string, IState>();
  }

  /// <summary>
  /// Get the State of the particular type
  /// </summary>
  /// <typeparam name="TState"></typeparam>
  /// <returns>The specific IState</returns>
  public TState GetState<TState>()
  {
    Type stateType = typeof(TState);
    return (TState)GetState(stateType);
  }

  /// <summary>
  /// Clear all the states
  /// </summary>
  public void Reset() => States.Clear();

  /// <summary>
  /// Set the state for specific Type
  /// </summary>
  /// <param name="newState"></param>
  public void SetState(IState newState)
  {
    string typeName = newState.GetType().FullName ?? throw new InvalidOperationException();
    SetState(typeName, newState);
  }

  public object GetState(Type stateType)
  {
    using (Logger.BeginScope(nameof(GetState)))
    {
      string className = GetType().Name;
      string typeName = stateType.FullName ?? throw new InvalidOperationException();

      if (!States.TryGetValue(typeName, out IState state))
      {
        Logger.LogDebug(EventIds.Store_CreateState, "Creating State of type: {typeName}", typeName);
        state = (IState)ServiceProvider.GetRequiredService(stateType);
        state.Initialize();
        States.Add(typeName, state);
        Publisher.Publish(new StateInitializedNotification(stateType)); // TODO: This is NOT await by intention, double check this is good idea.  I don't want this method async.
      }
      else
        Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, state.Guid);
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
