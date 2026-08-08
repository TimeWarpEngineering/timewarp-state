namespace Test.App.Client.Pipeline.NotificationPostProcessor;

// Non-generic so Mediator's source generator can emit Publish/handler wiring for it.
// (Open-generic INotification types produce invalid generated code.)
public class PostPipelineNotification : INotification
{
  public required object Request { get; init; }
  public required object? Response { get; init; }
}
