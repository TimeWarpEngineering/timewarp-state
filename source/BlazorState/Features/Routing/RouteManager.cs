namespace BlazorState.Features.Routing
{
  using System;
  using MediatR;
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.Components.Routing;

  /// <summary>
  /// When constructed will attach a OnLocationChanged Handler 
  /// to send ChangeRouteRequest
  /// </summary>
  public class RouteManager
  {
    public RouteManager(
      IUriHelper aUriHelper,
      IMediator aMediator,
      IStore aStore)
    {
      UriHelper = aUriHelper;
      Mediator = aMediator;
      Store = aStore;
      UriHelper.OnLocationChanged += OnLocationChanged;
      Mediator.Send(new InitializeRouteAction());
    }

    private IMediator Mediator { get; }
    private RouteState RouteState => Store.GetState<RouteState>();
    private IStore Store { get; }
    private IUriHelper UriHelper { get; }

    private void OnLocationChanged(object aSender, LocationChangedEventArgs aLocationChangedEventArgs)
    {
      string absoluteUri = UriHelper.ToAbsoluteUri(aLocationChangedEventArgs.Location).ToString();
      if (RouteState.Route != absoluteUri)
      {
        Mediator.Send(new ChangeRouteAction { NewRoute = absoluteUri });
      }
    }
  }
}