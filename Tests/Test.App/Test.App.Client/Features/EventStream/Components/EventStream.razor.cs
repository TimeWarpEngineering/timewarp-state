namespace Test.App.Client.Features.EventStream.Components;

public partial class EventStream : BaseComponent
{
  private IReadOnlyList<string> Events => EventStreamState.Events;
}
