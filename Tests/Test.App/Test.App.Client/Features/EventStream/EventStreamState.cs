namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState : State<EventStreamState>
{
  private List<string> EventList { get; set; } = [];
  public IReadOnlyList<string> Events => EventList.AsReadOnly();

  public override void Initialize() { }
}
