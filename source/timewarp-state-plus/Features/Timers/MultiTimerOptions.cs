namespace TimeWarp.State.Plus.Features.Timers;

public class MultiTimerOptions
{
  // ReSharper disable once CollectionNeverUpdated.Global Set from Configuration
  public Dictionary<string, TimerConfig> Timers { get; init; } = new();
}
