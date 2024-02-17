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
        NavigationManager.NavigateTo(RouteState.History.Pop());
      }
      return Task.CompletedTask;
    }
  }
}
