#region Purpose
// Contracts every state implements: the ClientPipeline Sender, Guid, Initialize and CancelOperations, plus
// IState<TState>.Hydrate for Redux DevTools time travel.
#endregion

#region Design
// Split in two so non-generic code (Store, behaviors) can work with IState while Hydrate stays strongly typed.
// IState<TState> is covariant.
#endregion

namespace TimeWarp.State;

public interface IState
{
  /// <summary>
  /// The store pipeline sender. Components and pipeline behaviors dispatch actions through
  /// the same <see cref="ClientPipeline"/> the store uses. Action handlers must not send
  /// actions; they publish notifications instead (TWS0002).
  /// </summary>
  ISender<ClientPipeline> Sender { get; set; }
  Guid Guid { get; }
  // string? CacheKey { get; }
  // DateTime? TimeStamp { get; }
  // TimeSpan CacheDuration { get; }
  // bool IsCacheValid(string currentCacheKey);

  void Initialize();
  public void CancelOperations();
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
