namespace BlazorState
{
  using System;
  using System.Collections.Generic;

  public interface IState
  {
    Guid Guid { get; }
  }

  public interface IState<TState> : IState
  {
    TState State { get; }

    /// <summary>
    /// Set the state from Dictionary
    /// Used by ReduxDevTools to support TimeTravel
    /// </summary>
    /// <param name="aKeyValuePairs"></param>
    /// <returns></returns>
    /// <remarks>Only needed for time travel which I think is waste anyway.</remarks>
    TState Hydrate(IDictionary<string, object> aKeyValuePairs);
  }
}