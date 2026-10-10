#region Purpose
// Owns named System.Timers.Timer instances and publishes TimerElapsedNotification when they fire.
#endregion

#region Design
// CreateTimer is the only place that constructs a Timer: Elapsed, AutoReset=false, Start, then store.
// Add/Update/Initialize all go through it so action-created timers publish the same way as option-seeded ones.
// CreateTimer Stop+Disposes a same-name replacement; Remove and Dispose walk the same Stop+Dispose path.
// Clone shares the Timers dictionary (transaction rollback must not duplicate running timers).
// Elapsed is async Task, not async void. Failures are logged. When the state is created on a
// synchronization context (the Blazor Server circuit), the publish is posted there. Otherwise
// TimerElapsedNotification handlers must marshal UI work with InvokeAsync.
// The context is captured when the Store first constructs the state, so first access from a
// thread-pool continuation captures none. Touch TimerState from the circuit to get marshalling.
// System.Timers.Timer stays: AutoReset false plus Start is the one-shot contract RestartTimer uses.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

using Microsoft.Extensions.Options;
using System.Timers;

public sealed partial class TimerState : State<TimerState>, ICloneable
{
  private readonly ILogger<TimerState> Logger;
  private readonly IPublisher<ClientPipeline> Publisher;
  private readonly MultiTimerOptions MultiTimerOptions;
  private readonly SynchronizationContext? CircuitContext = SynchronizationContext.Current;
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
    timer.Elapsed += (_, _) => DispatchElapsed(timerName);
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
  
  private void DispatchElapsed(string timerName)
  {
    if (CircuitContext is { } circuitContext)
    {
      circuitContext.Post(_ => _ = PublishElapsedAsync(timerName), null);
      return;
    }

    _ = PublishElapsedAsync(timerName);
  }

  internal async Task PublishElapsedAsync(string timerName)
  {
    try
    {
      Logger.LogInformation(EventIds.MultiTimerPostProcessor_TimerElapsed, message: "{TimerName} elapsed", timerName);
      TimerElapsedNotification notification = new(timerName, restartTimer: () => RestartTimer(timerName));
      await Publisher.Publish(notification, CancellationToken.None);
    }
    catch (Exception exception)
    {
      Logger.LogError
      (
        EventIds.MultiTimerPostProcessor_TimerElapsedFailed,
        exception,
        message: "Timer {TimerName} elapsed handler failed",
        timerName
      );
    }
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
