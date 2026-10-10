#region Purpose
// TimerState.PublishElapsedAsync publishes on a Task, logs handler failures, and the notification can restart a timer.
#endregion

#region Design
// Calls the internal publish method. A one-hour timer does not elapse during the test. The restart callback is the
// one the notification carries.
#endregion

namespace TimerElapsed_;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TimeWarp.State.Plus.Features.Timers;

public class TimerElapsed_Should
{
  public async Task Publish_The_Notification_And_Restart_The_Named_Timer()
  {
    ListLogger logger = new();
    RecordingPublisher publisher = new();
    using TimerState timerState = Create(logger, publisher, "hourly");

    await timerState.PublishElapsedAsync("hourly");

    publisher.Notifications.Count.ShouldBe(1);
    TimerElapsedNotification notification = publisher.Notifications[0].ShouldBeOfType<TimerElapsedNotification>();
    notification.TimerName.ShouldBe("hourly");

    notification.RestartTimer();

    logger.EventIds.ShouldContain(EventIds.MultiTimerPostProcessor_TimerRestarted.Id);
  }

  public async Task Log_And_Complete_When_A_Handler_Throws()
  {
    ListLogger logger = new();
    RecordingPublisher publisher = new() { ThrowOnPublish = true };
    using TimerState timerState = Create(logger, publisher, "hourly");

    await timerState.PublishElapsedAsync("hourly");

    logger.Levels.ShouldContain(LogLevel.Error);
    logger.EventIds.ShouldContain(EventIds.MultiTimerPostProcessor_TimerElapsedFailed.Id);
  }

  private static TimerState Create(ListLogger logger, RecordingPublisher publisher, string timerName)
  {
    MultiTimerOptions options = new();
    options.Timers[timerName] = new TimerConfig { Duration = 3_600_000, ResetOnActivity = false };
    TimerState timerState = new(Options.Create(options), logger, publisher);
    timerState.Initialize();
    return timerState;
  }

  private sealed class RecordingPublisher : IPublisher<ClientPipeline>
  {
    public List<object> Notifications { get; } = [];

    public bool ThrowOnPublish { get; init; }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
      if (ThrowOnPublish)
      {
        throw new InvalidOperationException("handler failed");
      }

      Notifications.Add(notification);
      return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification =>
      Publish((object)notification!, cancellationToken);
  }

  private sealed class ListLogger : ILogger<TimerState>
  {
    public List<LogLevel> Levels { get; } = [];

    public List<int> EventIds { get; } = [];

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Scope();

    private sealed class Scope : IDisposable
    {
      public void Dispose() {}
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>
    (
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter
    )
    {
      Levels.Add(logLevel);
      EventIds.Add(eventId.Id);
    }
  }
}
