namespace TestApp.Client.Pipeline.NotificationPreProcessor
{
  using MediatR;
  using MediatR.Pipeline;
  using Microsoft.Extensions.Logging;
  using System.Threading;
  using System.Threading.Tasks;

  internal class PrePipelineNotificationRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
  {
    private readonly ILogger Logger;
    private readonly IPublisher Publisher;

    public PrePipelineNotificationRequestPreProcessor
    (
      ILogger<PrePipelineNotificationRequestPreProcessor<TRequest>> aLogger,
      IPublisher aPublisher
    )
    {
      Logger = aLogger;
      Publisher = aPublisher;
    }

    public Task Process(TRequest aRequest, CancellationToken aCancellationToken)
    {
      var notification = new PrePipelineNotification<TRequest>
      {
        Request = aRequest,
      };

      Logger.LogDebug("PrePipelineNotificationRequestPreProcessor");
      return Publisher.Publish(notification, aCancellationToken);
    }
  }
}