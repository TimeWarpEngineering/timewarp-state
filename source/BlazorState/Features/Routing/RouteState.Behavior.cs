namespace BlazorState.Features.Routing
{
  using BlazorState;

  public partial class RouteState : State<RouteState>
  {
    public RouteState() { }

    protected RouteState(RouteState aState) : this()
    {
      Route = aState.Route;
    }

    public override object Clone() => new RouteState(this);

    protected override void Initialize() { }
  }
}