namespace Test.App.Client.Pipeline.NotificationPostProcessor;

internal class PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>
(
  ILogger<PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>> logger,
  IPublisher Publisher
) :
  IRequestPostProcessor<TRequest, TResponse>
  where TRequest : notnull
{
  private readonly ILogger Logger = logger;

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    var notification = new PostPipelineNotification<TRequest, TResponse>
    {
      Request = request,
      Response = response
    };

    Logger.LogDebug(nameof(PostPipelineNotificationRequestPostProcessor<TRequest, TResponse>));
    return Publisher.Publish(notification, cancellationToken);
  }
}
