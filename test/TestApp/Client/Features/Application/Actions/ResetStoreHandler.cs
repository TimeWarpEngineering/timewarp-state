namespace TestApp.Client.Features.Application
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorState.Features.Routing;
  using MediatR;

  internal class ResetStoreHandler : IRequestHandler<ResetStoreAction>
  {
    public ResetStoreHandler(IStore aStore, IMediator aMediator)
    {
      Mediator = aMediator;
      Store = aStore;
    }

    private IMediator Mediator { get; }
    private IStore Store { get; }

    public Task<Unit> Handle(ResetStoreAction aResetStoreAction, CancellationToken aCancellationToken)
    {
      Store.Reset();
      Mediator.Send(new ChangeRouteAction
      {
        NewRoute = "/"
      });
      return Unit.Task;
    }
  }
}
