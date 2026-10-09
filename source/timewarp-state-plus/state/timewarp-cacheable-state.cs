#region Purpose
// Base state that skips reloading data when the same request was handled within CacheDuration.
#endregion

#region Design
// The cache key is the action's type FullName plus its JSON serialization. HandleWithCaching runs the update only
// on a miss and records the key and a UTC timestamp. Derived states can update or invalidate the key.
// The class is State<TState> with where TState : TimeWarpCacheableState<TState>, so a derived state is
// IState of itself and Hydrate returns that state. StateInheritanceAnalyzer allows this abstract self-constrained
// intermediate; a concrete class must still pass itself to State<T>.
#endregion

namespace TimeWarp.State.Plus.State;

public abstract class TimeWarpCacheableState<TState> : State<TState>, ITimeWarpCacheableState
where TState : TimeWarpCacheableState<TState>
{
  public string? CacheKey { get; private set; }
  public DateTime? TimeStamp { get; private set; }
  public TimeSpan CacheDuration { get; protected set; }
  
  /// <summary>
  /// Checks if the cache is valid based on the current cache key and timestamp
  /// </summary>
  /// <param name="currentCacheKey">The cache key to validate against</param>
  /// <returns>True if the cache is valid, otherwise false</returns>
  protected bool IsCacheValid(string currentCacheKey)
  {
    return CacheKey == currentCacheKey &&
      TimeStamp.HasValue &&
      (DateTime.UtcNow - TimeStamp.Value) < CacheDuration;
  }

  // overload IsCacheValid to take in an IAction and serialize it to use as the cache key
  // use System.Text.Json to serialize the action to a string
  // public bool IsCacheValid(IAction action) => IsCacheValid(JsonSerializer.Serialize(action));

  protected static string GenerateCacheKey<TAction>(TAction action) where TAction : IAction
  {
    Type actionType = action.GetType();
    string actionProperties = JsonSerializer.Serialize(action);
    return $"{actionType.FullName}|{actionProperties}";
  }
  
  protected async Task HandleWithCaching<TAction>(TAction action, Func<TAction, CancellationToken, Task> updateStateFunc, CancellationToken cancellationToken) where TAction : IAction
  {
    string serializedAction = GenerateCacheKey(action);
    if (IsCacheValid(serializedAction)) return;

    await updateStateFunc(action, cancellationToken);

    CacheKey = serializedAction;
    TimeStamp = DateTime.UtcNow;
  }
  
  protected void UpdateCacheKey(string newCacheKey)
  {
    if (string.IsNullOrWhiteSpace(newCacheKey))
      throw new ArgumentException("Cache key cannot be null or empty", nameof(newCacheKey));
      
    CacheKey = newCacheKey;
    TimeStamp = DateTime.UtcNow;
  }
  
  protected void InvalidateCache()
  {
    CacheKey = null;
    TimeStamp = null;
  }
}
