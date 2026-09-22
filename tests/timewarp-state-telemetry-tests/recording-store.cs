namespace TimeWarp.State.Telemetry.Tests;

internal sealed class RecordingStore : IStore
{
  public Guid Guid { get; } = Guid.NewGuid();
  public IState CurrentState { get; set; }
  public int GetStateCallCount { get; private set; }
  public ConcurrentDictionary<string, Task> StateInitializationTasks { get; } = new();

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

  public object GetState(Type stateType)
  {
    GetStateCallCount++;
    return CurrentState;
  }

  public SemaphoreSlim? GetSemaphore(Type stateType) => null;

  public void SetState(IState newState)
  {
    CurrentState = newState;
  }

  public void RemoveState<TState>() where TState : IState { }

  public void Reset() { }
}
