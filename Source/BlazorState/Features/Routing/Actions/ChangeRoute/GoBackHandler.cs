namespace BlazorState.Features.Routing;

public partial class RouteState
{
  internal class GoBackHandler : ActionHandler<GoBackAction>
  {
    private readonly NavigationManager NavigationManager;
    
    public GoBackHandler
    (
      IStore aStore,
      NavigationManager aNavigationManager
    ) : base(aStore)
    {
      NavigationManager = aNavigationManager;
    }
    public override async Task Handle(GoBackAction aAction, CancellationToken aCancellationToken)
    {
      RouteState routeState = await Store.GetStateAsync<RouteState>();
      NavigationManager.NavigateTo(routeState.History.Pop());
    }
  }
}
