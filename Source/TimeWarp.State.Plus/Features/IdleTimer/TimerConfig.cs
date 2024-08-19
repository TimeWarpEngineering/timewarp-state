namespace TimeWarp.State.Plus.Features.IdleTimer;

public class TimerConfig
{
  public double Duration { get; set; }
  public bool ResetOnActivity { get; init; } = true;
}
