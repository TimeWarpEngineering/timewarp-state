namespace Test.App.Client.Pipeline.NotificationPreProcessor;

// Non-generic so Mediator's source generator can emit Publish/handler wiring for it.
// (Open-generic INotification types produce invalid generated code.)
public class PrePipelineNotification : INotification
{
  public required object Request { get; init; }
}
