#region Purpose
// Regression: AddTimer/UpdateTimer must wire Elapsed so TimerElapsedNotification is published.
#endregion

// ReSharper disable UnusedType.Global
namespace AddTimer_;

using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TimeWarp.Mediator;
using TimeWarp.State;
using TimeWarp.State.Plus.Features.Timers;

public class AddTimer_Should
{
  private const double ShortDurationMs = 50;
  private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(2);

  public async Task Publish_TimerElapsedNotification_When_Added_Via_Action()
  {
    using TimerHarness timerHarness = new();

    await timerHarness.AddHandler.Handle
    (
      new TimerState.AddTimerActionSet.Action
      (
        "SessionTimer",
        new TimerConfig { Duration = ShortDurationMs, ResetOnActivity = false }
      ),
      CancellationToken.None
    );

    TimerElapsedNotification notification = await timerHarness.Publisher.FirstElapsed.Task
      .WaitAsync(PublishTimeout);

    notification.TimerName.ShouldBe("SessionTimer");
    notification.RestartTimer.ShouldNotBeNull();
  }

  public async Task Not_Publish_After_Timer_Is_Removed()
  {
    using TimerHarness timerHarness = new();

    await timerHarness.AddHandler.Handle
    (
      new TimerState.AddTimerActionSet.Action
      (
        "RemovedTimer",
        new TimerConfig { Duration = ShortDurationMs, ResetOnActivity = false }
      ),
      CancellationToken.None
    );

    await timerHarness.RemoveHandler.Handle
    (
      new TimerState.RemoveTimerActionSet.Action("RemovedTimer"),
      CancellationToken.None
    );

    await Task.Delay(TimeSpan.FromMilliseconds(ShortDurationMs * 4));

    timerHarness.Publisher.NotificationCount.ShouldBe(0);
  }

  public async Task Not_Publish_Original_Duration_After_Update()
  {
    using TimerHarness timerHarness = new();

    await timerHarness.AddHandler.Handle
    (
      new TimerState.AddTimerActionSet.Action
      (
        "UpdatedTimer",
        new TimerConfig { Duration = ShortDurationMs, ResetOnActivity = false }
      ),
      CancellationToken.None
    );

    await timerHarness.UpdateHandler.Handle
    (
      new TimerState.UpdateTimerActionSet.Action
      (
        "UpdatedTimer",
        new TimerConfig { Duration = 30_000, ResetOnActivity = false }
      ),
      CancellationToken.None
    );

    await Task.Delay(TimeSpan.FromMilliseconds(ShortDurationMs * 4));

    timerHarness.Publisher.NotificationCount.ShouldBe(0);
  }

  private sealed class TimerHarness : IDisposable
  {
    public RecordingPublisher Publisher { get; }
    public TimerState TimerState { get; }
    public TimerState.AddTimerActionSet.Handler AddHandler { get; }
    public TimerState.UpdateTimerActionSet.Handler UpdateHandler { get; }
    public TimerState.RemoveTimerActionSet.Handler RemoveHandler { get; }

    public TimerHarness()
    {
      Publisher = new RecordingPublisher();
      TimerState = new
      (
        Options.Create(new MultiTimerOptions()),
        NullLogger<TimerState>.Instance,
        Publisher
      );

      IStore store = A.Fake<IStore>();
      A.CallTo(() => store.GetState<TimerState>()).Returns(TimerState);

      AddHandler = new(store);
      UpdateHandler = new(store);
      RemoveHandler = new(store);
    }

    public void Dispose() => TimerState.Dispose();
  }

  private sealed class RecordingPublisher : IPublisher<ClientPipeline>
  {
    private readonly object Gate = new();
    private readonly List<INotification> Notifications = [];
    public TaskCompletionSource<TimerElapsedNotification> FirstElapsed { get; } =
      new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int NotificationCount
    {
      get
      {
        lock (Gate)
        {
          return Notifications.Count;
        }
      }
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
      lock (Gate)
      {
        if (notification is INotification typedNotification)
        {
          Notifications.Add(typedNotification);
        }

        if (notification is TimerElapsedNotification timerElapsedNotification)
        {
          FirstElapsed.TrySetResult(timerElapsedNotification);
        }
      }

      return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification
    {
      return Publish((object)notification, cancellationToken);
    }
  }
}
