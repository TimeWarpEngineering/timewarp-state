namespace Test.App.Client.Features.Counter;

internal class IncrementCountNotificationHandler
(
  ILogger<IncrementCountNotificationHandler> logger
) : INotificationHandler<PostPipelineNotification>
{
  private readonly ILogger Logger = logger;

  public Task Handle
  (
    PostPipelineNotification postPipelineNotification,
    CancellationToken cancellationToken
  )
  {
    // The notification is now non-generic and fires for every action; keep the original
    // behavior of only reacting to the IncrementCount action.
    if (postPipelineNotification.Request is not CounterState.IncrementCountActionSet.Action) return Task.CompletedTask;

    Logger.LogDebug("{postPipelineNotification_Request_Type_Name}", postPipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{postPipelineNotification_Response_Type_Name}", postPipelineNotification.Response?.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
