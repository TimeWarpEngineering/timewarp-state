namespace TimeWarp.State.Plus.State;

public abstract class BaseCacheableState<TState> : State<TState>, ICacheableState
{
  public string? CacheKey { get; protected init; }
  public DateTime? TimeStamp { get; protected init; }
  public TimeSpan CacheDuration { get; protected init; }
  
  
  /// <summary>
  /// Checks if the cache is valid based on the current cache key and timestamp
  /// </summary>
  /// <param name="currentCacheKey">The cache key to validate against</param>
  /// <returns>True if the cache is valid, otherwise false</returns>
  public bool IsCacheValid(string currentCacheKey) => 
    CacheKey == currentCacheKey &&
    TimeStamp.HasValue &&
    (DateTime.UtcNow - TimeStamp.Value) < CacheDuration;
}

