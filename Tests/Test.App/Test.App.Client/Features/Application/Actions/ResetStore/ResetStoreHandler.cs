namespace Test.App.Client.Features.Application;

using static RouteState;
using static ApplicationState;

// ReSharper disable once UnusedType.Global
internal class ResetStoreHandler
(
  IStore Store,
  ISender Sender
) : IRequestHandler<ResetStoreAction>
{
  public async Task Handle(ResetStoreAction aResetStoreAction, CancellationToken aCancellationToken)
  {
    Store.Reset();
    await Sender.Send(new ChangeRoute.Action { NewRoute = "/" }, aCancellationToken);
  }
}
