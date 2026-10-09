#region Purpose
// Configuration for one timer: duration, and whether user activity resets it.
#endregion

#region Design
// Plain options class; ResetOnActivity defaults to true.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

public class TimerConfig
{
  public double Duration { get; set; }
  public bool ResetOnActivity { get; init; } = true;
}
