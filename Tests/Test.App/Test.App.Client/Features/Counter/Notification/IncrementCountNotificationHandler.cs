namespace Test.App.Client.Features.Counter;

[UsedImplicitly]
internal class IncrementCountNotificationHandler
(
  ILogger<IncrementCountNotificationHandler> logger
) : INotificationHandler<PostPipelineNotification<CounterState.IncrementCountActionSet.Action, Unit>>
{
  private readonly ILogger Logger = logger;

  public Task Handle
  (
    PostPipelineNotification<CounterState.IncrementCountActionSet.Action, Unit> postPipelineNotification,
    CancellationToken cancellationToken
  ) 
  {
    Logger.LogDebug("{postPipelineNotification_Request_Type_Name}", postPipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{postPipelineNotification_Response_Type_Name}", postPipelineNotification.Response.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
