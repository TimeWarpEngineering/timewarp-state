#region Purpose
// Per-scope bag of IState instances, keyed by type name.
#endregion

#region Design
// GetState uses a per-type lock so concurrent first access does not throw.
// The lock serializes construction: Initialize and StateInitializedNotification run on the
// canonical instance only, and the instance is inserted only after Initialize succeeds so a
// TryGetValue hit is always initialized. RemoveState and Reset take the same lock.
// Reset removes every key the way RemoveState does: cancel, drop previous state, drop the
// initialization task. A later GetState starts a new instance and a new initialization task.
// Reset walks every key in States, PreviousStates and StateInitializationTasks. A throwing
// CancelOperations does not stop the walk; failures are rethrown together afterwards.
// StateInitializationTasks stays internal. IStore exposes WaitForInitializationAsync and
// FindInitializationTask instead of the dictionary.
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
  private readonly ConcurrentDictionary<string, object> StateInitializationLocks;
  private readonly IPublisher<ClientPipeline> Publisher;
  private readonly TimeWarpStateOptions TimeWarpStateOptions;

  /// <summary>
  /// Unique Guid for the Store.
  /// </summary>
  /// <remarks>Useful when logging </remarks>
  public Guid Guid { get; } = Guid.NewGuid();
  internal ConcurrentDictionary<string, Task> StateInitializationTasks { get; } = new();

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
    RemoveStateByName(typeName);
  }

  /// <summary>
  /// Remove every state the way <see cref="RemoveState{TState}"/> does.
  /// </summary>
  public void Reset()
  {
    string[] typeNames = States.Keys
      .Concat(PreviousStates.Keys)
      .Concat(StateInitializationTasks.Keys)
      .Distinct()
      .ToArray();

    List<Exception>? exceptions = null;
    foreach (string typeName in typeNames)
    {
      try
      {
        RemoveStateByName(typeName);
      }
      catch (Exception exception)
      {
        (exceptions ??= []).Add(exception);
      }
    }

    if (exceptions is not null)
    {
      throw new AggregateException(exceptions);
    }
  }

  public Task WaitForInitializationAsync<TState>() where TState : IState
  {
    _ = GetState<TState>();
    return FindInitializationTask(typeof(TState)) ?? Task.CompletedTask;
  }

  public Task? FindInitializationTask(Type stateType)
  {
    ArgumentNullException.ThrowIfNull(stateType);
    string typeName = stateType.FullName ?? throw new InvalidOperationException();
    return StateInitializationTasks.TryGetValue(typeName, out Task? initializationTask)
      ? initializationTask
      : null;
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

  public IState GetState(Type stateType)
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

  private void RemoveStateByName(string typeName)
  {
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
      StateInitializationTasks.TryRemove(typeName, out _);
    }
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
