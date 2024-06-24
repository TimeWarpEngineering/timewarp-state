namespace TimeWarp.State;

/// <summary>
/// 
/// </summary>
internal partial class Store : IStore
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly ILogger Logger;
  private readonly IServiceProvider ServiceProvider;
  private readonly IDictionary<string, IState> States;
  private readonly IDictionary<string, SemaphoreSlim> Semaphores;
  private readonly IPublisher Publisher;
  private readonly TimeWarpStateOptions TimeWarpStateOptions;

  /// <summary>
  /// Unique Guid for the Store.
  /// </summary>
  /// <remarks>Useful when logging </remarks>
  public Guid Guid { get; } = Guid.NewGuid();

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
    Semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
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
  /// Get the Semaphore for the specific State
  /// </summary>
  public SemaphoreSlim GetSemaphore(Type stateType)
  {
    string typeName = stateType.FullName ?? throw new InvalidOperationException();
    if (Semaphores.TryGetValue(typeName, out SemaphoreSlim? semaphore)) return semaphore;
    semaphore = new SemaphoreSlim(1, 1);
    Semaphores.Add(typeName, semaphore);
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
        state = (IState)ServiceProvider.GetRequiredService(stateType);
        state.Initialize();
        States.Add(typeName, state);
        
        // Fire-and-forget publishing the state initialization notification with exception handling
        Task.Run(async () =>
        {
          try
          {
            await Publisher.Publish(new StateInitializedNotification(stateType));
          }
          catch (Exception ex)
          {
            Logger.LogError(ex, "Error occurred while publishing state initialization notification.");
          }
        });
      }
      else
        Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, state.Guid);
      return state;
    }
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
    States[typeName] = newState;
  }
}
