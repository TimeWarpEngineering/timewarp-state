namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class InitializeRoute
  {
    [UsedImplicitly]
    public class Action : IAction {}

    internal class InitializeRouteHandler : ActionHandler<InitializeRoute.Action>
    {
      private readonly NavigationManager NavigationManager;
      public InitializeRouteHandler
      (
        IStore store,
        NavigationManager navigationManager
      ) : base(store)
      {
        NavigationManager = navigationManager;
      }
      private RouteState RouteState => Store.GetState<RouteState>();

      public override Task Handle(InitializeRoute.Action action, CancellationToken cancellationToken)
      {
        RouteState.Route = NavigationManager.Uri;
        return Task.CompletedTask;
      }
    }
  }
}
