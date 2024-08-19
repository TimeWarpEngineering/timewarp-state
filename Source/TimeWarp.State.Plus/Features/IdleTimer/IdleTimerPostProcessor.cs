namespace TimeWarp.State.Plus.Features.IdleTimer;

using System.Timers;
using Microsoft.Extensions.Options;

public class IdleTimerPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : notnull
{
  private readonly ILogger<IdleTimerPostProcessor<TRequest, TResponse>> Logger;
  private readonly Timer Timer;
  private readonly IPublisher Publisher;
  private bool IsDisposed;

  public IdleTimerPostProcessor
  (
    ILogger<IdleTimerPostProcessor<TRequest, TResponse>> logger,
    IOptions<IdleTimerOptions> options,
    IPublisher publisher
  )
  {
    Logger = logger;
    Publisher = publisher;
    double timeoutDuration = options.Value.TimeoutDuration;

    Timer = new Timer(timeoutDuration);
    Timer.Elapsed += OnTimerElapsed;
    Timer.AutoReset = false;
    Timer.Start();
    Logger.LogDebug("Idle timer started with timeout of {TimeoutDuration} ms", timeoutDuration);
  }

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    Timer.Stop();
    Timer.Start();// Reset timer on activity
    Logger.LogDebug("Idle timer reset");

    return Task.CompletedTask;
  }

  private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
  {
    Logger.LogInformation("Idle timeout elapsed");
    Publisher.Publish(new IdleTimeoutElapsed());
  }
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (IsDisposed) return;
    if (disposing) Timer?.Dispose();
    IsDisposed = true;
  }
}

internal class IdleTimeoutElapsed:INotification;

public class IdleTimerOptions
{
  public double TimeoutDuration { get; set; } = 600000; // Default to 10 min
}

namespace TimeWarp.State.Plus.Features.IdleTimer;

using System.Timers;

public class IdleTimerPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>, IDisposable
  where TRequest : notnull
{
  private readonly ILogger<IdleTimerPostProcessor<TRequest, TResponse>> Logger;
  private readonly NavigationManager NavigationManager;
  private readonly IStore Store;
  private Timer? Timer;
  private bool IsDisposed;

  private AuthorizationState AuthorizationState => Store.GetState<AuthorizationState>();

  public IdleTimerPostProcessor
  (
    IStore store,
    ILogger<IdleTimerPostProcessor<TRequest, TResponse>> logger,
    NavigationManager navigationManager
  )
  {
    Store = store;
    Logger = logger;
    NavigationManager = navigationManager;
  }

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    if (AuthorizationState.IdleTimeoutDuration.HasValue)
    {
      var timespan = TimeSpan.FromSeconds(AuthorizationState.IdleTimeoutDuration.Value);

      if (Timer is null)
      {
        CreateTimer(timespan);
      }
      else
      {
        if (Timer.Interval != timespan.TotalMilliseconds)
        {
          Timer.Dispose();
          CreateTimer(timespan);
        }
        else
        {
          Timer.Stop();
          Timer.Start(); // Reset timer on activity
          Logger.LogDebug("Idle timer reset");
        }
      }
    }

    return Task.CompletedTask;
  }

  private void CreateTimer(TimeSpan timespan)
  {
    Timer = new Timer(timespan.TotalMilliseconds);
    Timer.Elapsed += OnTimerElapsed;
    Timer.AutoReset = false;
    Timer.Start();
    Logger.LogDebug("Idle timer started with timeout of {TimeoutDuration} seconds", timespan.TotalSeconds);
  }

  private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
  {
    Logger.LogInformation("Idle timeout elapsed, navigating to logout");
    NavigationManager.NavigateToLogout("authentication/logout");
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (IsDisposed) return;
    if (disposing) Timer?.Dispose();
    IsDisposed = true;
  }
}
