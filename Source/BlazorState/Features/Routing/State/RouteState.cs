namespace BlazorState.Features.Routing;

using BlazorState;

/// <summary>
/// Maintain the Route in Blazor-State
/// </summary>
public partial class RouteState : State<RouteState>
{
  private Stack<string> History = new();

  public string Route { get; private set; }

  public override void Initialize() { }
}
