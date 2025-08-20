namespace TimeWarp.State.Plus.Features.Timers;

using Microsoft.Extensions.Options;
using System.Timers;

public sealed partial class TimerState : State<TimerState>, ICloneable
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

  /// <summary>
  /// Creates a new instance of TimerState with the same configuration and Timer instances.
  /// </summary>
  /// <remarks>
  /// This method performs a shallow clone of the state.
  /// It reuses the existing Timer instances and configuration.
  /// If an error occurs in an action, it will not rollback to the previous state.
  /// This approach is intentional to maintain consistency of timer states across clones.
  /// Actions are expected to be well-tested and reliable, minimizing the risk of failures.
  /// </remarks>
  /// <returns>A new TimerState instance with the same configuration and Timer instances.</returns>
  public object Clone()
  {
    return new TimerState
    (
      new OptionsWrapper<MultiTimerOptions>(MultiTimerOptions),
      Logger,
      Publisher
    )
    {
      Timers = this.Timers
    };
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
