namespace TestApp.Client.Pipeline.NotificationPreProcessor
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;
  using MediatR.Pipeline;
  using Microsoft.Extensions.Logging;

  internal class PrePipelineNotificationRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
  {
    public PrePipelineNotificationRequestPreProcessor
    (
      ILogger<PrePipelineNotificationRequestPreProcessor<TRequest>> aLogger,
      IMediator aMediator
    )
    {
      Logger = aLogger;
      Mediator = aMediator;
    }

    private ILogger Logger { get; }
    private IMediator Mediator { get; }

    public async Task Process(TRequest aRequest, CancellationToken aCancellationToken)
    {
      var notification = new PrePipelineNotification<TRequest>
      {
        Request = aRequest,
      };

      Logger.LogDebug("PrePipelineNotificationRequestPreProcessor");
      await Mediator.Publish(notification, aCancellationToken);
    }
  }
}