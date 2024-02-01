namespace Test.App.Client.Features.EventStream.Components;

using System.Collections.Generic;
using Test.App.Client.Features.Base.Components;

public partial class EventStream : BaseComponent
{
  public IReadOnlyList<string> Events => EventStreamState.Events;
}
