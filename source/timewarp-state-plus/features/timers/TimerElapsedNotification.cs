namespace TimeWarp.State.Plus.Features.Timers;

public class TimerElapsedNotification : INotification
{
  public string TimerName { get; }
  public Action RestartTimer { get; }

  public TimerElapsedNotification(string timerName, Action restartTimer)
  {
    TimerName = timerName;
    RestartTimer = restartTimer;
  }
}
