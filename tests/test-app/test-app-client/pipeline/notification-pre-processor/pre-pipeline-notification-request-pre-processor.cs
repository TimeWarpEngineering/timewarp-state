namespace Test.App.Client.Pipeline.NotificationPreProcessor;

internal sealed class PrePipelineNotificationRequestPreProcessor<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
  where TMessage : IAction
{
  private readonly ILogger Logger;
  private readonly IPublisher Publisher;
  public PrePipelineNotificationRequestPreProcessor
  (
    ILogger<PrePipelineNotificationRequestPreProcessor<TMessage, TResponse>> logger,
    IPublisher publisher
  )
  {
    Publisher = publisher;
    Logger = logger;
  }

  protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
  {
    var notification = new PrePipelineNotification
    {
      Request = message
    };

    Logger.LogDebug(nameof(PrePipelineNotificationRequestPreProcessor<TMessage, TResponse>));
    return Publisher.Publish(notification, cancellationToken);
  }
}
