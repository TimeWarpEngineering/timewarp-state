namespace BlazorState.Features.Routing;

public partial class RouteState
{
  internal class GoBackHandler : ActionHandler<GoBackAction>
  {
    private readonly NavigationManager NavigationManager;

    private RouteState RouteState => Store.GetState<RouteState>();

    public GoBackHandler
    (
      IStore aStore,
      NavigationManager aNavigationManager
    ) : base(aStore)
    {
      NavigationManager = aNavigationManager;
    }
    public override Task Handle(GoBackAction aAction, CancellationToken aCancellationToken)
    {
      NavigationManager.NavigateTo(RouteState.History.Pop());
      return Task.CompletedTask;
    }
  }
}
