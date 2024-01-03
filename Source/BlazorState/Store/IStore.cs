namespace BlazorState;

public interface IReduxDevToolsStore
{
  IDictionary<string, object> GetSerializableState();

  void LoadStatesFromJson(string aJsonString);
}

public interface IStore
{
  Guid Guid { get; }

  Task<TState> GetStateAsync<TState>();

  Task<object> GetStateAsync(Type aType);

  void SetState(IState aState);

  void Reset();
}
