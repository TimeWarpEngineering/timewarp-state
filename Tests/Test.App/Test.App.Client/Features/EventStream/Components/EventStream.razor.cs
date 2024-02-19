namespace Test.App.Client.Features.EventStream.Components;

using System.Collections.Generic;

public partial class EventStream : BaseComponent
{
  private IReadOnlyList<string> Events => EventStreamState.Events;
}
