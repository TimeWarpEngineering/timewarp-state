namespace TimeWarp.State;

public interface IState
{
  ISender Sender { get; set; }
  Guid Guid { get; }
  // string? CacheKey { get; }
  // DateTime? TimeStamp { get; }
  // TimeSpan CacheDuration { get; }
  // bool IsCacheValid(string currentCacheKey);

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
