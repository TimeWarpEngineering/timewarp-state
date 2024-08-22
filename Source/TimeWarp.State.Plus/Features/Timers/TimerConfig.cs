namespace TimeWarp.State.Plus.Features.Timers;

public class TimerConfig
{
  public double Duration { get; set; }
  public bool ResetOnActivity { get; init; } = true;
}
