namespace TestApp.Client.Features.EventStream;

using BlazorState;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Client.Features.Base;


internal partial class EventStreamState
{
  internal class AddEventHandler : BaseActionHandler<AddEventAction>
  {
    public AddEventHandler(IStore store) : base(store) { }

    public override Task Handle
    (
      AddEventAction aAddEventAction,
      CancellationToken aCancellationToken
    )
    {
      EventStreamState._Events.Add(aAddEventAction.Message);
      return Task.CompletedTask;
    }
  }
}
