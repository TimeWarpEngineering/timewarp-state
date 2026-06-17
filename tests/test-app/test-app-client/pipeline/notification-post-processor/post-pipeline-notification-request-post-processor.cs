namespace Test.App.Client.Pipeline.NotificationPostProcessor;

internal class PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>
(
  ILogger<PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>> logger,
  IPublisher Publisher
) :
  MessagePostProcessor<TRequest, TResponse>
  where TRequest : IMessage
{
  private readonly ILogger Logger = logger;

  protected override ValueTask Handle(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    var notification = new PostPipelineNotification
    {
      Request = request,
      Response = response
    };

    Logger.LogDebug(nameof(PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>));
    return Publisher.Publish(notification, cancellationToken);
  }
}
