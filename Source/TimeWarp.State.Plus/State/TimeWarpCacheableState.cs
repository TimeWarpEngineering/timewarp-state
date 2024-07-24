namespace TimeWarp.State.Plus.State;

public abstract class TimeWarpCacheableState<TState> : State<TState>, ITimeWarpCacheableState
where TState : IState
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

  protected void UpdateCacheDuration(TimeSpan newDuration)
  {
    if (newDuration < TimeSpan.Zero)
    {
      throw new ArgumentException("Cache duration cannot be negative", nameof(newDuration));
    }
    CacheDuration = newDuration;
  }
}
