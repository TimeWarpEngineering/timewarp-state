namespace BlazorState.Features.Routing.ChangeRoute
{
  using MediatR;
  public class Request : IRequest<RouteState>
  {
    public string NewRoute { get; set; }
  }
}