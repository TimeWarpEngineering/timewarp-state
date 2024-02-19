namespace BlazorState.Features.Routing;

public partial class RouteState
{
  internal class GoBackHandler
  (
    IStore store,
    NavigationManager NavigationManager
  ) : ActionHandler<GoBackAction>(store)
  {

    private RouteState RouteState => Store.GetState<RouteState>();

    public override Task Handle(GoBackAction aAction, CancellationToken aCancellationToken)
    {
      if (RouteState.History.Count != 0)
      {
        RouteState.GoingBack = true;
        NavigationManager.NavigateTo(RouteState.HistoryStack.Pop());
      }
      return Task.CompletedTask;
    }
  }
}
