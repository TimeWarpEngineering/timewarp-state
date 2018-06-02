namespace BlazorState.Features.Routing
{
  using BlazorState;

  /// <summary>
  /// Maintain the Route in Blazor-State
  /// </summary>
  public class RouteState : State<RouteState>
  {
    public RouteState() { }

    protected RouteState(RouteState aState) : this()
    {
      Route = aState.Route;
    }

    public string Route { get; set; }

    public override object Clone() => new RouteState(this);

    protected override void Initialize() { }
  }
}