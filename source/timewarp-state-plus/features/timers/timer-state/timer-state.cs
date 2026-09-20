#region Purpose
// Owns named System.Timers.Timer instances and publishes TimerElapsedNotification when they fire.
#endregion

#region Design
// CreateTimer is the only place that constructs a Timer: Elapsed, AutoReset=false, Start, then store.
// Add/Update/Initialize all go through it so action-created timers publish the same way as option-seeded ones.
// CreateTimer Stop+Disposes a same-name replacement; Remove and Dispose walk the same Stop+Dispose path.
// Clone shares the Timers dictionary (transaction rollback must not duplicate running timers).
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

using Microsoft.Extensions.Options;
using System.Timers;

public sealed partial class TimerState : State<TimerState>, ICloneable
{
  private readonly ILogger<TimerState> Logger;
  private readonly IPublisher<ClientPipeline> Publisher;
  private readonly MultiTimerOptions MultiTimerOptions;
  private Dictionary<string, (Timer Timer, TimerConfig TimerConfig)> Timers = new();

  public TimerState
  (
    IOptions<MultiTimerOptions> multiTimerOptionsAccessor,
    ILogger<TimerState> logger,
    IPublisher<ClientPipeline> publisher
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
    foreach ((_, (Timer timer, TimerConfig _)) in Timers)
    {
      StopAndDispose(timer);
    }
    Timers.Clear();
    // Load from options
    foreach ((string timerName, TimerConfig timerConfig) in MultiTimerOptions.Timers)
    {
      CreateTimer(timerName, timerConfig);
    }
  }

  private void CreateTimer(string timerName, TimerConfig timerConfig)
  {
    if (Timers.TryGetValue(timerName, out (Timer Timer, TimerConfig TimerConfig) existing))
    {
      StopAndDispose(existing.Timer);
    }

    Timer timer = new(timerConfig.Duration);
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

  private static void StopAndDispose(Timer timer)
  {
    timer.Stop();
    timer.Dispose();
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

  protected override void Dispose(bool disposing)
  {
    if (IsDisposed) return;
    if (disposing)
    {
      foreach ((_, (Timer timer, TimerConfig _)) in Timers)
      {
        StopAndDispose(timer);
      }
      Timers.Clear();
    }

    base.Dispose(disposing);
  }
}
