namespace TimeWarp.State.Plus.Features.Timers;

using System.Timers;

public partial class TimerState : State<TimerState>
{
  private readonly ILogger<TimerState> Logger;
  private readonly IPublisher Publisher;
  private readonly MultiTimerOptions MultiTimerOptions;
  private Dictionary<string, (Timer Timer, TimerConfig TimerConfig)> Timers = new();
  public TimerState
  (
    IOptions<MultiTimerOptions> multiTimerOptionsAccessor,
    ILogger<TimerState> logger,
    IPublisher publisher
  )
  {
    Logger = logger;
    Publisher = publisher;
    MultiTimerOptions = multiTimerOptionsAccessor.Value;
  }
  public override void Initialize()
  {
    Timers.Clear();
    // Load from options
    foreach ((string timerName, TimerConfig timerConfig) in MultiTimerOptions.Timers)
    {
      var timer = new Timer(timerConfig.Duration);
      timer.Elapsed += (_, _) => OnTimerElapsed(timerName);
      timer.AutoReset = false;
      timer.Start();
      Timers[timerName] = (timer, timerConfig);
      Logger.LogDebug
      (
        EventIds.MultiTimerPostProcessor_TimerStarted,
        message: "{TimerName} started with timeout of {TimeoutDuration} ms, ResetOnActivity: {ResetOnActivity}",
        timerName,
        timerConfig.Duration,
        timerConfig.ResetOnActivity
      );
    }
  }
  
  private async void OnTimerElapsed(string timerName)
  {
    Logger.LogInformation(EventIds.MultiTimerPostProcessor_TimerElapsed, message: "{TimerName} elapsed", timerName);
    var notification = new TimerElapsedNotification(timerName, restartTimer: () => RestartTimer(timerName));
    await Publisher.Publish(notification, CancellationToken.None);
  }
  
  private void RestartTimer(string timerName)
  {
    if (Timers.TryGetValue(timerName, out (Timer Timer, TimerConfig TimerConfig) timerData))
    {
      (Timer timer, TimerConfig _) = timerData;
      timer.Stop();
      timer.Start();
      Logger.LogDebug(EventIds.MultiTimerPostProcessor_TimerRestarted, message: "{TimerName} restarted", timerName);
    }
    else
    {
      Logger.LogWarning
      (
        EventIds.MultiTimerPostProcessor_TimerRestartAttemptFailed, 
        message: "Attempted to restart non-existent timer: {TimerName}",
        timerName
      );
    }
  }
}
