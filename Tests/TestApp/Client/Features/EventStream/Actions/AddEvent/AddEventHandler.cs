namespace TestApp.Client.Features.EventStream
{
  using BlazorState;
  using MediatR;
  using System;
  using System.Runtime.Serialization;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base;


  internal partial class EventStreamState
  {
    internal class AddEventHandler : BaseHandler<AddEventAction>
    {
      public AddEventHandler(IStore aStore) : base(aStore) { }

      public override Task<Unit> Handle
      (
        AddEventAction aAddEventAction,
        CancellationToken aCancellationToken
      )
      {
        EventStreamState._Events.Add(aAddEventAction.Message);
        return Unit.Task;
      }
    }
  }
}