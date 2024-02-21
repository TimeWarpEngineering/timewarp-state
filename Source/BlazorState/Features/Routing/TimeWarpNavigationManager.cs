namespace BlazorState.Features.Routing;

/// <summary>
/// When constructed will attach a OnLocationChanged Handler
/// to send ChangeRouteRequest
/// </summary>
public class TimeWarpNavigationManager
{
  private readonly IMediator Mediator;

  private readonly NavigationManager NavigationManager;

  private readonly IStore Store;

  private RouteState RouteState => Store.GetState<RouteState>();

  public TimeWarpNavigationManager
  (
    NavigationManager aNavigationManager,
    IMediator aMediator,
    IStore aStore
  )
  {
    NavigationManager = aNavigationManager;
    Mediator = aMediator;
    Store = aStore;
    NavigationManager.LocationChanged += LocationChanged;
    Mediator.Send(new RouteState.InitializeRouteAction());
  }

  private void LocationChanged(object aSender, LocationChangedEventArgs aLocationChangedEventArgs)
  {
    string absoluteUri = NavigationManager.ToAbsoluteUri(aLocationChangedEventArgs.Location).ToString();
    if (RouteState.Route != absoluteUri)
    {
      Mediator.Send(new RouteState.ChangeRoute.Action { NewRoute = absoluteUri });
    }
  }
}
