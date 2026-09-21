#region Purpose
// Per-scope bag of IState instances, keyed by type name, with per-type SemaphoreSlim gates.
#endregion

#region Design
// GetState/GetSemaphore use ConcurrentDictionary.GetOrAdd so concurrent first access does not throw.
// Per-type locks serialize construction: Initialize and StateInitializedNotification run on the
// canonical instance only, and the instance is inserted only after Initialize succeeds so a
// TryGetValue hit is always initialized. RemoveState takes the same lock. Reset clears States.
#endregion

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
  private readonly ConcurrentDictionary<string, object> StateInitializationLocks;
  private readonly IPublisher<ClientPipeline> Publisher;
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
    IPublisher<ClientPipeline> publisher
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
    StateInitializationLocks = new ConcurrentDictionary<string, object>();
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
    object initializationLock = StateInitializationLocks.GetOrAdd(typeName, static _ => new object());
    lock (initializationLock)
    {
      Logger.LogDebug
      (
        EventIds.Store_RemoveState,
        "{Timestamp:O} Removing State: {TypeName}",
        DateTime.UtcNow,
        typeName
      );
      PreviousStates.Remove(typeName, out _);
      States.Remove(typeName, out IState? state);
      state?.CancelOperations();

      // Remove and dispose the associated Semaphore
      if (Semaphores.TryRemove(typeName, out SemaphoreSlim? semaphore))
      {
        semaphore.Dispose();
      }

      // Optionally, remove the initialization task
      StateInitializationTasks.TryRemove(typeName, out _);
    }
  }

  /// <summary>
  /// Clear all the states
  /// </summary>
  public void Reset() => States.Clear();

  /// <summary>
  /// Get the Semaphore for the specific State
  /// </summary>
  public SemaphoreSlim? GetSemaphore(Type stateType)
  {
    string typeName = stateType.FullName ?? throw new InvalidOperationException();
    if (Semaphores.TryGetValue(typeName, out SemaphoreSlim? existing))
    {
      return existing;
    }

    if (!States.ContainsKey(typeName))
    {
      return null;
    }

    SemaphoreSlim created = new(1, 1);
    SemaphoreSlim semaphore = Semaphores.GetOrAdd(typeName, created);
    if (!ReferenceEquals(semaphore, created))
    {
      created.Dispose();
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

      if (States.TryGetValue(typeName, out IState? existingState))
      {
        Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, existingState.Guid);
        return existingState;
      }

      object initializationLock = StateInitializationLocks.GetOrAdd(typeName, static _ => new object());
      lock (initializationLock)
      {
        if (States.TryGetValue(typeName, out existingState))
        {
          Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, existingState.Guid);
          return existingState;
        }

        Logger.LogDebug(EventIds.Store_CreateState, "Creating State of type: {typeName}", typeName);

        IState created = (IState)ServiceProvider.GetRequiredService(stateType);
        created.Sender = ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
        created.Initialize();

        IState state = States.GetOrAdd(typeName, created);
        if (!ReferenceEquals(state, created))
        {
          if (created is IDisposable disposable)
          {
            disposable.Dispose();
          }

          Logger.LogDebug(EventIds.Store_GetState, "State of type ({typeName}) exists with Guid: {state_Guid}", typeName, state.Guid);
          return state;
        }

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
        return state;
      }
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
    
    // Check if the state exists before trying to access it
    // If the state has been removed then does it make sense to keep this new one? 
    if (States.TryGetValue(typeName, out var currentState))
    {
      Logger.LogDebug
      (
        EventIds.Store_SetState,
        "Assigning State. Type:{typeName}, Guid:{newState.Guid}",
        typeName,
        newState.Guid
      );
      PreviousStates[typeName] = currentState;
      States[typeName] = newState;
    }
    else
    {
      Logger.LogDebug("State was removed while processing. Type:{typeName}", typeName);
    }
  }
}
