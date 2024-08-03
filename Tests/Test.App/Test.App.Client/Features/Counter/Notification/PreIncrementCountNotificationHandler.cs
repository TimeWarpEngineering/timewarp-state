namespace Test.App.Client.Features.Counter;

[UsedImplicitly]
internal class PreIncrementCountNotificationHandler
(
  ILogger<PreIncrementCountNotificationHandler> logger
) : INotificationHandler<PrePipelineNotification<CounterState.IncrementCountActionSet.Action>>
{
  private readonly ILogger Logger = logger;

  public Task Handle
  (
    PrePipelineNotification<CounterState.IncrementCountActionSet.Action> prePipelineNotification,
    CancellationToken cancellationToken
  )
  {
    Logger.LogDebug("{prePipelineNotification_Request_Type_Name}", prePipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
