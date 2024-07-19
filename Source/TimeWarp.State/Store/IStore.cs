namespace TimeWarp.State;

public interface IReduxDevToolsStore
{
  IDictionary<string, object> GetSerializableState();

  void LoadStatesFromJson(string jsonString);
}

public interface IStore
{
  Guid Guid { get; }

  TState GetState<TState>();
  
  TState GetPreviousState<TState>();

  object GetState(Type stateType);
  
  SemaphoreSlim GetSemaphore(Type stateType);

  void SetState(IState newState);

  void Reset();
}
