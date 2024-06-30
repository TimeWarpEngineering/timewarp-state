namespace TimeWarp.State.Plus.State;

public interface ICacheableState
{
  string? CacheKey { get; }
  DateTime? TimeStamp { get; }
  TimeSpan CacheDuration { get;} 
}
