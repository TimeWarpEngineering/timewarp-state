namespace TimeWarp.State;

public interface IReduxDevToolsStore
{
  IDictionary<string, object> GetSerializableState();

  void LoadStatesFromJson(string jsonString);
}

public interface IStore
{
  Guid Guid { get; }

  TState GetState<TState>() where TState : IState;
  
  TState? GetPreviousState<TState>() where TState : IState;

  object GetState(Type stateType);
  
  SemaphoreSlim? GetSemaphore(Type stateType);

  void SetState(IState newState);
  
  void RemoveState<TState>() where TState : IState;

  void Reset();
  
  ConcurrentDictionary<string, Task> StateInitializationTasks { get; }
}
