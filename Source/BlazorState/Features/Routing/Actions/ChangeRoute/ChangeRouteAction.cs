namespace BlazorState.Features.Routing
{
  using BlazorState;

  public partial class RouteState
  {
    public class ChangeRouteAction : IAction
    {
      public string NewRoute { get; set; }
    }
  }
}