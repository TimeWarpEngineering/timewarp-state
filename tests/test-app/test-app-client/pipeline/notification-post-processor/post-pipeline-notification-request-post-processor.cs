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
