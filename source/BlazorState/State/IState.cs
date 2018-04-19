namespace BlazorState.State
{
  using System;

  public interface IState : ICloneable
  {
    Guid Guid { get; }
  }

  public interface IState<TState> : IState
  {
    TState State { get; }

    TState Hydrate(string aJsonString);
  }
}