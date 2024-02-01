namespace Test.App.Client.Features.Counter;

using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

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
