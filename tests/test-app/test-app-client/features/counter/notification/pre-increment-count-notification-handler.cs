namespace Test.App.Client.Features.Counter;

internal class PreIncrementCountNotificationHandler
(
  ILogger<PreIncrementCountNotificationHandler> logger
) : INotificationHandler<PrePipelineNotification>
{
  private readonly ILogger Logger = logger;

  public Task Handle
  (
    PrePipelineNotification prePipelineNotification,
    CancellationToken cancellationToken
  )
  {
    // The notification is now non-generic and fires for every action; keep the original
    // behavior of only reacting to the IncrementCount action.
    if (prePipelineNotification.Request is not CounterState.IncrementCountActionSet.Action) return Task.CompletedTask;

    Logger.LogDebug("{prePipelineNotification_Request_Type_Name}", prePipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
