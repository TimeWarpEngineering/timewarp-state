namespace Test.App.Client.Pipeline.NotificationPreProcessor;

public class PrePipelineNotification<TRequest> : INotification
{
  public TRequest Request { get; set; }
}
