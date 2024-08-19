namespace TimeWarp.State.Plus.Features.IdleTimer;
using System.Timers;

public class MultiTimerPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>, IDisposable
  where TRequest : notnull
{
  private readonly ILogger<MultiTimerPostProcessor<TRequest, TResponse>> Logger;
  private readonly IMediator Mediator;
  private readonly Dictionary<string, (Timer Timer, TimerConfig Config)> Timers;
  private bool IsDisposed;

  public MultiTimerPostProcessor
  (
    ILogger<MultiTimerPostProcessor<TRequest, TResponse>> logger,
    IOptions<MultiTimerOptions> options,
    IMediator mediator
  )
  {
    Logger = logger;
    Mediator = mediator;
    Timers = new Dictionary<string, (Timer, TimerConfig)>();

    foreach ((string timerName, TimerConfig config) in options.Value.Timers)
    {
      var timer = new Timer(config.Duration);
      timer.Elapsed += (sender, e) => OnTimerElapsed(timerName);
      timer.AutoReset = false;
      timer.Start();
      Timers[timerName] = (timer, config);
      Logger.LogDebug("{TimerName} started with timeout of {TimeoutDuration} ms, ResetOnActivity: {ResetOnActivity}", 
        timerName, config.Duration, config.ResetOnActivity);
    }
  }

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    foreach ((string timerName, (Timer timer, TimerConfig config)) in Timers)
    {
      if (config.ResetOnActivity)
      {
        RestartTimer(timerName);
      }
    }

    return Task.CompletedTask;
  }

  private async void OnTimerElapsed(string timerName)
  {
    Logger.LogInformation("{TimerName} elapsed", timerName);
    var notification = new TimerElapsedNotification(timerName, () => RestartTimer(timerName));
    await Mediator.Publish(notification);
  }

  private void RestartTimer(string timerName)
  {
    if (Timers.TryGetValue(timerName, out (Timer Timer, TimerConfig Config) timerData))
    {
      (Timer timer, TimerConfig config) = timerData;
      timer.Stop();
      timer.Interval = config.Duration;
      timer.Start();
      Logger.LogDebug("{TimerName} restarted", timerName);
    }
    else
    {
      Logger.LogWarning("Attempted to restart non-existent timer: {TimerName}", timerName);
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

public class MultiTimerOptions
{
  public Dictionary<string, TimerConfig> Timers { get; set; } = new();
}

public class TimerConfig
{
  public double Duration { get; set; }
  public bool ResetOnActivity { get; set; } = true;
}

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
