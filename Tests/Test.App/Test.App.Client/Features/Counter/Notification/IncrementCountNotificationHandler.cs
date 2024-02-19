namespace Test.App.Client.Features.Counter;

internal class IncrementCountNotificationHandler
  : INotificationHandler<PostPipelineNotification<CounterState.IncrementCounterAction, Unit>>
{
  private readonly ILogger Logger;

  public IncrementCountNotificationHandler(ILogger<IncrementCountNotificationHandler> aLogger)
  {
    Logger = aLogger;
  }

  public Task Handle
  (
    PostPipelineNotification<CounterState.IncrementCounterAction, Unit> aPostPipelineNotification,
    CancellationToken aCancellationToken
  )
  {
    Logger.LogDebug("{aPostPipelineNotification_Request_Type_Name}", aPostPipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
