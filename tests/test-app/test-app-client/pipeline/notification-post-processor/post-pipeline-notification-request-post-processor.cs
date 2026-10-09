#region Purpose
// Pipeline behavior that publishes a PostPipelineNotification after each request completes.
#endregion

#region Design
// Registered at order 510. Awaits next() first, then publishes the request and response through
// IPublisher<ClientPipeline>; an exception from next() skips the notification.
#endregion

namespace Test.App.Client.Pipeline.NotificationPostProcessor;

internal class PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>
(
  ILogger<PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>> logger,
  IPublisher<ClientPipeline> Publisher
) :
  IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  private readonly ILogger Logger = logger;

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    TResponse response = await next(cancellationToken);

    var notification = new PostPipelineNotification
    {
      Request = request,
      Response = response
    };

    Logger.LogDebug(nameof(PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>));
    await Publisher.Publish(notification, cancellationToken);
    return response;
  }
}
