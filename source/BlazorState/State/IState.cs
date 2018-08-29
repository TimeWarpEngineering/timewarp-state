namespace BlazorState
{
  using System;
  using System.Collections.Generic;

  public interface IState : ICloneable
  {
    Guid Guid { get; }
  }

  public interface IState<TState> : IState
  {
    TState State { get; }

    TState Hydrate(IDictionary<string, object> aKeyValuePairs);
  }
}