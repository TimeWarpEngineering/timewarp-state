namespace Test.App.Client.Features.Counter;

[UsedImplicitly]
internal class PreIncrementCountNotificationHandler
(
  ILogger<PreIncrementCountNotificationHandler> logger
) : INotificationHandler<PrePipelineNotification<CounterState.IncrementCounterAction>>
{
  private readonly ILogger Logger = logger;

  public Task Handle
  (
    PrePipelineNotification<CounterState.IncrementCounterAction> prePipelineNotification,
    CancellationToken aCancellationToken
  )
  {
    Logger.LogDebug("{prePipelineNotification_Request_Type_Name}", prePipelineNotification.Request.GetType().Name);
    Logger.LogDebug("{methodName} handled", nameof(IncrementCountNotificationHandler));
    return Task.CompletedTask;
  }
}
