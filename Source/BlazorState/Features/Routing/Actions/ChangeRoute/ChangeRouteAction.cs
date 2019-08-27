namespace BlazorState.Features.Routing
{
  using MediatR;

  public class ChangeRouteAction : IRequest<RouteState>
  {
    public string NewRoute { get; set; }
  }
}