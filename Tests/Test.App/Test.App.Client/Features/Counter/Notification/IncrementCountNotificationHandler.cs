namespace Test.App.Client.Features.Counter;

[UsedImplicitly]
internal class IncrementCountNotificationHandler
(
  ILogger<IncrementCountNotificationHandler> logger
) : INotificationHandler<PostPipelineNotification<CounterState.IncrementCounterAction, Unit>>
{
  private readonly ILogger Logger = logger;

  public Task Handle
  (
    PostPipelineNotification<CounterState.IncrementCounterAction, Unit> aPostPipelineNotification,
    CancellationToken aCancellationToken
  )
  {
    Logger.LogDebug("{aPostPipelineNotification_Request_Type_Name}", aPostPipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{aPostPipelineNotification_Response_Type_Name}", aPostPipelineNotification.Response.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
