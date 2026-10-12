#region Purpose
// Store contract: get, set, remove and reset states, previous states, and initialization wait.
#endregion

#region Design
// GetState(Type) returns IState. Callers wait with WaitForInitializationAsync. The task dictionary stays on
// the store implementation. There is no public per-state semaphore.
#endregion

namespace TimeWarp.State;

public interface IStore
{
  Guid Guid { get; }

  TState GetState<TState>() where TState : IState;

  TState? GetPreviousState<TState>() where TState : IState;

  IState GetState(Type stateType);

  void SetState(IState newState);

  void RemoveState<TState>() where TState : IState;

  void Reset();

  /// <summary>
  /// Ensures <typeparamref name="TState"/> exists and returns the task that completes when its
  /// <c>StateInitializedNotification</c> has finished. The task is already completed when initialization
  /// finished earlier.
  /// </summary>
  Task WaitForInitializationAsync<TState>() where TState : IState;

  /// <summary>
  /// Returns the initialization task for a state that has already been created, or null when it has not.
  /// Does not create the state.
  /// </summary>
  Task? FindInitializationTask(Type stateType);
}
