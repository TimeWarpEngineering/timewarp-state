namespace TestApp.Client.Features.Counter
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  using MediatR;
  using Microsoft.Extensions.Logging;
  using TestApp.Client.Pipeline.NotificationPostProcessor;

  internal class IncrementCountNotificationHandler
    : INotificationHandler<PostPipelineNotification<IncrementCounterAction, CounterState>>
  {
    public IncrementCountNotificationHandler(ILogger<IncrementCountNotificationHandler> aLogger)
    {
      Logger = aLogger;
    }
    private ILogger Logger { get; }

    public Task Handle(PostPipelineNotification<IncrementCounterAction, CounterState> aNotification, CancellationToken aCancellationToken)
    {
      Logger.LogDebug(aNotification.Request.GetType().Name);
      Logger.LogDebug($"{nameof(IncrementCountNotificationHandler)} handled");
      return Task.CompletedTask;
    }
  }
}
