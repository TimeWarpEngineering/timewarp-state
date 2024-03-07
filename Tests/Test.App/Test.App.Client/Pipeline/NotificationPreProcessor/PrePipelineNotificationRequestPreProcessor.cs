namespace Test.App.Client.Pipeline.NotificationPreProcessor;

internal class PrePipelineNotificationRequestPreProcessor<TRequest>
(
  ILogger<PrePipelineNotificationRequestPreProcessor<TRequest>> logger,
  IPublisher Publisher
) : IRequestPreProcessor<TRequest>
  where TRequest : IAction
{
  private readonly ILogger Logger = logger;

  public Task Process(TRequest request, CancellationToken cancellationToken)
  {
    var notification = new PrePipelineNotification<TRequest>
    {
      Request = request
    };

    Logger.LogDebug(nameof(PrePipelineNotificationRequestPreProcessor<TRequest>));
    return Publisher.Publish(notification, cancellationToken);
  }
}
