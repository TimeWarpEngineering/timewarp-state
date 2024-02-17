namespace BlazorState.Features.Routing;

public partial class RouteState
{
  public class ChangeRouteAction : IAction
  {
    public string NewRoute { get; init; }
  }
}
