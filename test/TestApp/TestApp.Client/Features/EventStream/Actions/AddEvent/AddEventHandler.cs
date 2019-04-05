namespace TestApp.Client.Features.EventStream
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using TestApp.Client.Features.Base;


  internal partial class EventStreamState
  {
    internal class AddEventHandler : BaseHandler<AddEventAction, EventStreamState>
    {
      public AddEventHandler(IStore aStore) : base(aStore) { }

      public override Task<EventStreamState> Handle(
        AddEventAction aAddEventAction, 
        CancellationToken aCancellationToken)
      {
        EventStreamState.Events.Add(aAddEventAction.Message);
        return Task.FromResult(EventStreamState);
      }
    }
  }
}
