namespace TimeWarp.State;

public interface IState
{
  Guid Guid { get; }
  void Initialize();
}

public interface IState<out TState> : IState
{

  /// <summary>
  /// Set the state from Dictionary
  /// Used by ReduxDevTools to support TimeTravel
  /// </summary>
  /// <param name="keyValuePairs"></param>
  /// <returns></returns>
  /// <remarks>Only needed for time travel which I think is waste anyway.</remarks>
  TState Hydrate(IDictionary<string, object> keyValuePairs);
}
