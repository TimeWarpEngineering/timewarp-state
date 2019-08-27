namespace TestApp.Client.Pipeline.NotificationPostProcessor
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;
  using MediatR.Pipeline;
  using Microsoft.Extensions.Logging;

  internal class PostPipelineNotificationRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  {
    public PostPipelineNotificationRequestPostProcessor
    (
      ILogger<PostPipelineNotificationRequestPostProcessor<TRequest,TResponse>> aLogger,
      IMediator aMediator
    )
    {
      Logger = aLogger;
      Mediator = aMediator;
    }

    private IMediator Mediator { get; }
    private ILogger Logger { get; }

    public async Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
    {
      var notification = new PostPipelineNotification<TRequest, TResponse>
      {
        Request = aRequest,
        Response = aResponse
      };

      Logger.LogDebug("PostPipelineNotificationRequestPostProcessor");
      await Mediator.Publish(notification, aCancellationToken);
    }
  }
}