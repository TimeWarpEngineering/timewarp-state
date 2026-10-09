#region Purpose
// EventStreamState: the list of pipeline events recorded by EventStreamBehavior.
#endregion

#region Design
// The list is private with a read-only Events view, so only AddEvent and the test-only Initialize can change it.
// Initialize does nothing.
#endregion

namespace Test.App.Client.Features.EventStream;

public sealed partial class EventStreamState : State<EventStreamState>
{
  private List<string> EventList { get; set; } = [];
  public IReadOnlyList<string> Events => EventList.AsReadOnly();

  public override void Initialize() { }
}
