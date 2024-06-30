namespace Test.App.Client.Features.Application;

using static RouteState;

public partial class ApplicationState
{
  public class ResetStoreAction : IAction;
  
  [UsedImplicitly]
  internal class ResetStoreHandler : IRequestHandler<ResetStoreAction>
  {
    private readonly IStore Store;
    private readonly ISender Sender;
    public ResetStoreHandler(IStore store, ISender sender)
    {
      Store = store;
      Sender = sender;
    }
    public async Task Handle(ResetStoreAction resetStoreAction, CancellationToken cancellationToken)
    {
      Store.Reset();
      await Sender.Send(new ChangeRoute.Action(newRoute:"/"), cancellationToken);
    }
  }
}
