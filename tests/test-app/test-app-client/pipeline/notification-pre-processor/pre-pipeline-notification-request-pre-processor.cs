namespace Test.App.Client.Pipeline.NotificationPreProcessor;

internal sealed class PrePipelineNotificationRequestPreProcessor<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
  where TMessage : notnull, IAction
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

  public async Task<TResponse> Handle(TMessage message, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var notification = new PrePipelineNotification
    {
      Request = message
    };

    Logger.LogDebug(nameof(PrePipelineNotificationRequestPreProcessor<TMessage, TResponse>));
    await Publisher.Publish(notification, cancellationToken);
    return await next(cancellationToken);
  }
}
