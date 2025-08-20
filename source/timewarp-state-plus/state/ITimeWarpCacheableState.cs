namespace TimeWarp.State.Plus.State;

public interface ITimeWarpCacheableState
{
  string? CacheKey { get; }
  DateTime? TimeStamp { get; }
  TimeSpan CacheDuration { get;} 
}
