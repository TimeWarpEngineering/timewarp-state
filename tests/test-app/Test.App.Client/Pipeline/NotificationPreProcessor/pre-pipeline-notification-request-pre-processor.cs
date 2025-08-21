namespace Test.App.Client.Pipeline.NotificationPreProcessor;

internal class PrePipelineNotificationRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
  where TRequest : IAction
{
  private readonly ILogger Logger;
  private readonly IPublisher Publisher;
  public PrePipelineNotificationRequestPreProcessor
  (
    ILogger<PrePipelineNotificationRequestPreProcessor<TRequest>> logger,
    IPublisher publisher
  )
  {
    Publisher = publisher;
    Logger = logger;
  }

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
