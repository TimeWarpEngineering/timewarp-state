namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  internal class GoBackHandler
  (
    IStore store,
    NavigationManager NavigationManager
  ) : ActionHandler<GoBack.Action>(store)
  {
    private RouteState RouteState => Store.GetState<RouteState>();

    public override Task Handle(GoBack.Action action, CancellationToken cancellationToken)
    {
      if (RouteState.IsHistoryEmpty) return Task.CompletedTask;
      
      RouteState.GoingBack = true;
      NavigationManager.NavigateTo(RouteState.HistoryStack.Pop());
      return Task.CompletedTask;
    }
  }
}
