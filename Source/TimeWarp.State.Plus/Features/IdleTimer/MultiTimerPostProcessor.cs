namespace TimeWarp.State.Plus.Features.IdleTimer;
using System.Timers;

public class MultiTimerPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>, IDisposable
  where TRequest : notnull
{
  private readonly ILogger<MultiTimerPostProcessor<TRequest, TResponse>> Logger;
  private readonly IPublisher Publisher;
  private readonly Dictionary<string, (Timer Timer, TimerConfig TimerConfig)> Timers;
  private bool IsDisposed;

  public MultiTimerPostProcessor
  (
    ILogger<MultiTimerPostProcessor<TRequest, TResponse>> logger,
    IOptions<MultiTimerOptions> options,
    IPublisher publisher
  )
  {
    Logger = logger;
    Publisher = publisher;
    Timers = new Dictionary<string, (Timer, TimerConfig)>();

    foreach ((string timerName, TimerConfig timerConfig) in options.Value.Timers)
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

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    Logger.LogDebug(EventIds.MultiTimerPostProcessor_ProcessingRequest, message: "Processing request and checking timers");
    foreach ((string timerName, (Timer _, TimerConfig timerConfig)) in Timers)
    {
      if (timerConfig.ResetOnActivity)
      {
        RestartTimer(timerName);
      }
    }

    return Task.CompletedTask;
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

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (IsDisposed) return;
    if (disposing)
    {
      foreach ((Timer timer, TimerConfig _) in Timers.Values)
      {
        timer.Dispose();
      }
    }
    IsDisposed = true;
  }
}
