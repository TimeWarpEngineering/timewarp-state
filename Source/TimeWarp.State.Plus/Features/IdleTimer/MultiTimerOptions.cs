namespace TimeWarp.State.Plus.Features.IdleTimer;

public class MultiTimerOptions
{
  public Dictionary<string, TimerConfig> Timers { get; init; } = new();
}
