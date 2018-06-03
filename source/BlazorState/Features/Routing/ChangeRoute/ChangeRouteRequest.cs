namespace BlazorState.Features.Routing
{
  using MediatR;

  internal class ChangeRouteRequest : IRequest<RouteState>
  {
    public string NewRoute { get; set; }
  }
}