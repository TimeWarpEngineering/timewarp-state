namespace TestApp.Client.Features.Application
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using static BlazorState.Features.Routing.RouteState;

  internal class ResetStoreHandler : IRequestHandler<ResetStoreAction>
  {
    public ResetStoreHandler(IStore aStore, IMediator aMediator)
    {
      Mediator = aMediator;
      Store = aStore;
    }

    private IMediator Mediator { get; }
    private IStore Store { get; }

    public async Task<Unit> Handle(ResetStoreAction aResetStoreAction, CancellationToken aCancellationToken)
    {
      Store.Reset();
      _ = await Mediator.Send(new ChangeRouteAction { NewRoute = "/" });
      return Unit.Value;
    }
  }
}
