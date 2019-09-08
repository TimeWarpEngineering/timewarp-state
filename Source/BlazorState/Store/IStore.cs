namespace BlazorState
{
  using System;
  using System.Collections.Generic;

  public interface IReduxDevToolsStore
  {
    IDictionary<string, object> GetSerializableState();

    void LoadStatesFromJson(string aJsonString);
  }

  public interface IStore
  {
    Guid Guid { get; }

    TState GetState<TState>();

    object GetState(Type aType);

    void SetState(IState aState);

    void Reset();
  }
}