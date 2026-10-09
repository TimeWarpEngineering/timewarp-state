#region Purpose
// Proves StartHandler logs its own EventIds and that Handle is already complete when it returns.
#endregion

#region Design
// A capturing ILogger records every Log call. The constructor must use StartHandler_Initializing (500) and Handle
// must use StartHandler_RequestReceived (501). JumpToStateHandler_RequestHandled (512) must not appear.
#endregion

namespace StartHandlerTests;

public class Should_
{
  public void Log_StartHandler_Initializing_From_The_Constructor()
  {
    CapturingLogger logger = new();

    StartHandler handler = new(logger);

    handler.ShouldNotBeNull();
    logger.EventIds.ShouldBe([EventIds.StartHandler_Initializing]);
    logger.EventIds.ShouldAllBe(eventId => eventId.Id != EventIds.JumpToStateHandler_RequestHandled.Id);
  }

  public void Log_StartHandler_RequestReceived_And_Complete_Synchronously()
  {
    CapturingLogger logger = new();
    StartHandler handler = new(logger);
    StartRequest request = new(1, new StartRequest.PayloadClass(), "source", "{}", "START");

    Task task = handler.Handle(request, CancellationToken.None);

    task.IsCompletedSuccessfully.ShouldBeTrue();
    logger.EventIds.ShouldBe
    (
      [
        EventIds.StartHandler_Initializing,
        EventIds.StartHandler_RequestReceived
      ]
    );
    logger.EventIds.ShouldAllBe(eventId => eventId.Id != EventIds.JumpToStateHandler_RequestHandled.Id);
  }

  private sealed class CapturingLogger : ILogger<StartHandler>
  {
    public List<EventId> EventIds { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

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
      EventIds.Add(eventId);
    }
  }
}
