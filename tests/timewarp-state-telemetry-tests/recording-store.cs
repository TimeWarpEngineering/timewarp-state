#region Purpose
// IStore double for telemetry tests that counts GetState calls.
#endregion

#region Design
// Holds one current state and returns it for any type. GetStateCallCount lets tests prove TelemetryBehavior does not
// read state when no listener or sampling is active.
#endregion

namespace TimeWarp.State.Telemetry.Tests;

internal sealed class RecordingStore : IStore
{
  public Guid Guid { get; } = Guid.NewGuid();
  public IState CurrentState { get; set; }
  public int GetStateCallCount { get; private set; }

  public RecordingStore(IState currentState)
  {
    CurrentState = currentState;
  }

  public TState GetState<TState>() where TState : IState
  {
    GetStateCallCount++;
    return (TState)CurrentState;
  }

  public TState? GetPreviousState<TState>() where TState : IState => default;

  public IState GetState(Type stateType)
  {
    GetStateCallCount++;
    return CurrentState;
  }

  public Task WaitForInitializationAsync<TState>() where TState : IState => Task.CompletedTask;

  public Task? FindInitializationTask(Type stateType) => null;

  public void SetState(IState newState)
  {
    CurrentState = newState;
  }

  public void RemoveState<TState>() where TState : IState { }

  public void Reset() { }
}
