namespace TestApp.Client.Features.EventStream
{
  using System.Collections.Generic;
  using BlazorState;

  internal partial class EventStreamState : State<EventStreamState>
  {
    public EventStreamState()
    {
      Events = new List<string>();
    }

    private EventStreamState(EventStreamState aEventStreamState)
    {
      Events = new List<string>(aEventStreamState.Events);
    }

    public override object Clone() => new EventStreamState(this);

    /// <summary>
    /// Set the Initial State
    /// </summary>
    protected override void Initialize() { }
  }
}
