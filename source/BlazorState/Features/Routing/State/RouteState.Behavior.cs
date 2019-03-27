namespace BlazorState.Features.Routing
{
  using BlazorState;

  public partial class RouteState : State<RouteState>
  {
    public RouteState() { }

    protected RouteState(RouteState aRouteState) : this()
    {
      Route = aRouteState.Route;
    }

    public override object Clone() => new RouteState(this);

    protected override void Initialize() { }
  }
}