namespace Test.App.Client.Features.Application;

using static BlazorState.Features.Routing.RouteState;
using static Test.App.Client.Features.Application.ApplicationState;

internal class ResetStoreHandler
(
  IStore Store,
  ISender Sender
) : IRequestHandler<ResetStoreAction>
{


  public async Task Handle(ResetStoreAction aResetStoreAction, CancellationToken aCancellationToken)
  {
    Store.Reset();
    await Sender.Send(new ChangeRouteAction { NewRoute = "/" }, aCancellationToken);
  }
}
