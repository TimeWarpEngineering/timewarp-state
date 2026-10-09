#region Purpose
// Notification published before an action runs, carrying the request.
#endregion

#region Design
// Non-generic (object Request) because Mediator's source generator emits invalid code for open-generic INotification
// types; handlers filter by request type.
#endregion

namespace Test.App.Client.Pipeline.NotificationPreProcessor;

// Non-generic so Mediator's source generator can emit Publish/handler wiring for it.
// (Open-generic INotification types produce invalid generated code.)
public class PrePipelineNotification : INotification
{
  public required object Request { get; init; }
}
