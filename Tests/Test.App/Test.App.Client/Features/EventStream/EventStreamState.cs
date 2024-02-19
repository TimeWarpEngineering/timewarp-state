namespace Test.App.Client.Features.EventStream;

using System.Collections.Generic;

internal partial class EventStreamState : State<EventStreamState>
{
  private List<string> _Events { get; set; } = [];
  public IReadOnlyList<string> Events => _Events.AsReadOnly();

  public override void Initialize() { }
}
