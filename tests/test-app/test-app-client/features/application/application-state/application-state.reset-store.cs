namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  public static class ResetStoreActionSet
  {
    internal sealed class Action : IAction;
    
    internal sealed class Handler : IRequestHandler<Action>
    {
      private readonly IStore Store;
      public Handler(IStore store)
      {
        Store = store;
      }
      public async ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
      {
        Store.Reset();
        await Store.GetState<RouteState>().ChangeRoute(newRoute: "/", cancellationToken);
        return Unit.Value;
      }
    }
  }
}
