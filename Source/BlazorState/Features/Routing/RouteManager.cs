namespace BlazorState.Features.Routing
{
  using MediatR;
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.Components.Routing;

  /// <summary>
  /// When constructed will attach a OnLocationChanged Handler
  /// to send ChangeRouteRequest
  /// </summary>
  public class RouteManager
  {
    private readonly IMediator Mediator;

    private readonly NavigationManager NavigationManager;

    private readonly IStore Store;

    private RouteState RouteState => Store.GetState<RouteState>();

    public RouteManager(
                      NavigationManager aNavigationManager,
      IMediator aMediator,
      IStore aStore)
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
        Mediator.Send(new RouteState.ChangeRouteAction { NewRoute = absoluteUri });
      }
    }
  }
}