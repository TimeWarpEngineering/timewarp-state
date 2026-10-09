#region Purpose
// Contract for states that cache data: cache key, timestamp and duration.
#endregion

#region Design
// Read-only interface so callers can inspect cache status without depending on TimeWarpCacheableState.
#endregion

namespace TimeWarp.State.Plus.State;

public interface ITimeWarpCacheableState
{
  string? CacheKey { get; }
  DateTime? TimeStamp { get; }
  TimeSpan CacheDuration { get;} 
}
