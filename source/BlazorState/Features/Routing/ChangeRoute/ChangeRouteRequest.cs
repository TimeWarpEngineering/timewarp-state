namespace BlazorState.Features.Routing
{
  using MediatR;

  public class ChangeRouteRequest : IRequest<RouteState>
  {
    public string NewRoute { get; set; }
  }
}