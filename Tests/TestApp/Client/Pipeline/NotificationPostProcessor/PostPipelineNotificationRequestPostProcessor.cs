namespace TestApp.Client.Pipeline.NotificationPostProcessor
{
  using MediatR;
  using MediatR.Pipeline;
  using Microsoft.Extensions.Logging;
  using System.Threading;
  using System.Threading.Tasks;

  internal class PostPipelineNotificationRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  {
    private readonly ILogger Logger;
    private readonly IPublisher Publisher;

    public PostPipelineNotificationRequestPostProcessor
            (
      ILogger<PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>> aLogger,
      IPublisher aMediator
    )
    {
      Logger = aLogger;
      Publisher = aMediator;
    }

    public Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
    {
      var notification = new PostPipelineNotification<TRequest, TResponse>
      {
        Request = aRequest,
        Response = aResponse
      };

      Logger.LogDebug("PostPipelineNotificationRequestPostProcessor");
      return Publisher.Publish(notification, aCancellationToken);
    }
  }
}