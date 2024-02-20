namespace Test.App.Client.Pipeline.NotificationPreProcessor;

public class PrePipelineNotification<TRequest> : INotification
{
  public required TRequest Request { get; init; }
}
