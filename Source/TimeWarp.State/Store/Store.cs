namespace TimeWarp.State;

/// <summary>
/// 
/// </summary>
internal partial class Store : IStore
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly ILogger Logger;
  private readonly IServiceProvider ServiceProvider;
  private readonly ConcurrentDictionary<string, IState> States;
  private readonly ConcurrentDictionary<string, IState> PreviousStates;
  private readonly ConcurrentDictionary<string, SemaphoreSlim> Semaphores;
  private readonly IPublisher Publisher;
  private readonly TimeWarpStateOptions TimeWarpStateOptions;

  /// <summary>
  /// Unique Guid for the Store.
  /// </summary>
  /// <remarks>Useful when logging </remarks>
  public Guid Guid { get; } = Guid.NewGuid();
  public ConcurrentDictionary<string, Task> StateInitializationTasks { get; } = new();

  public Store
  (
    ILogger<Store> logger,
    IServiceProvider serviceProvider,
    TimeWarpStateOptions timeWarpStateOptions,
    IPublisher publisher
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.Store_Constructing, "constructing {StoreName} with guid:{Guid}", nameof(Store), Guid);
    ServiceProvider = serviceProvider;
    Publisher = publisher;
    TimeWarpStateOptions = timeWarpStateOptions;
    JsonSerializerOptions = timeWarpStateOptions.JsonSerializerOptions;

    States = new ConcurrentDictionary<string, IState>();
    PreviousStates = new ConcurrentDictionary<string, IState>();
    Semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
  }

  /// <summary>
  /// Get the State of the particular type
  /// </summary>
  /// <typeparam name="TState"></typeparam>
  /// <returns>The specific IState</returns>
  public TState GetState<TState>() where TState : IState
  {
    Type stateType = typeof(TState);
    return (TState)GetState(stateType);
  }

  public TState? GetPreviousState<TState>() where TState : IState
  {
    Type stateType = typeof(TState);
    return (TState?)GetPreviousState(stateType);
  }

  public void RemoveState<TState>() where TState : IState
  {
    string typeName = typeof(TState).FullName ?? throw new InvalidOperationException();
    Logger.LogDebug(EventIds.Store_RemoveState, "Removing State: {TypeName}", typeName);
    PreviousStates.Remove(typeName, out _);
    States.Remove(typeName, out _);
  }
  
  /// <summary>
  /// Clear all the states
  /// </summary>
  public void Reset() => States.Clear();

  /// <summary>
  /// Get the Semaphore for the specific State
  /// </summary>
  public SemaphoreSlim GetSemaphore(Type stateType)
  {
    string typeName = stateType.FullName ?? throw new InvalidOperationException();
    if (Semaphores.TryGetValue(typeName, out SemaphoreSlim? semaphore)) return semaphore;
    semaphore = new SemaphoreSlim(1, 1);
    if (!Semaphores.TryAdd(typeName, semaphore))
    {
      throw new InvalidOperationException($"An element with the key '{typeName}' already exists in the Semaphores dictionary.");  
    }
    return semaphore;
  }
  
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
      string typeName = stateType.FullName ?? throw new InvalidOperationException();

      if (!States.TryGetValue(typeName, out IState? state))
      {
        Logger.LogDebug(EventIds.Store_CreateState, "Creating State of type: {typeName}", typeName);

        // will use default constructor if none exists
        state = (IState)ServiceProvider.GetRequiredService(stateType);

        // we need to set the sender if the default constructor was used
        state.Sender = ServiceProvider.GetRequiredService<ISender>();

        state.Initialize();
        if (!States.TryAdd(typeName, state))
        {
          throw new InvalidOperationException($"An element with the key '{typeName}' already exists in the States dictionary.");
        }

        // Publish the state initialization notification asynchronously
        Task initializationTask = Publisher.Publish(new StateInitializedNotification(stateType))
          .ContinueWith
          (
            t =>
            {
              if (t.Exception != null)
              {
                Logger.LogError(t.Exception, "Error occurred while publishing state initialization notification.");
              }
            }, 
            TaskScheduler.Default
          );
        
        StateInitializationTasks[typeName] = initializationTask;
      }
      else
      {
        Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, state.Guid);
      }

      return state;
    }
  }
  
  public object? GetPreviousState(Type stateType)
  {
    string typeName = stateType.FullName ?? throw new InvalidOperationException();
    PreviousStates.TryGetValue(typeName, out IState? state);
    return state;
  }

  private void SetState(string typeName, object newStateObject)
  {
    var newState = (IState)newStateObject;
    Logger.LogDebug
    (
      EventIds.Store_SetState,
      "Assigning State. Type:{typeName}, Guid:{newState.Guid}",
      typeName,
      newState.Guid
    );
    PreviousStates[typeName] = States[typeName];
    States[typeName] = newState;
  }
}
