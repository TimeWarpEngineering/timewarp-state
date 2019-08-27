namespace TestApp.Client.Features.EventStream.Components
{
  using System.Collections.Generic;
  using TestApp.Client.Features.Base.Components;

  public class EventStreamModel : BaseComponent
  {
    public IReadOnlyList<string> Events => EventStreamState.Events;
  }
}
