#region Purpose
// Notification published after a request completes, carrying the request and its response.
#endregion

#region Design
// Non-generic (object Request and Response) because Mediator's source generator emits invalid code for open-generic
// INotification types; handlers filter by request type.
#endregion

namespace Test.App.Client.Pipeline.NotificationPostProcessor;

// Non-generic so Mediator's source generator can emit Publish/handler wiring for it.
// (Open-generic INotification types produce invalid generated code.)
public class PostPipelineNotification : INotification
{
  public required object Request { get; init; }
  public required object? Response { get; init; }
}
