namespace BlazorState.Features.Routing
{
  using System.Collections.Generic;
  using BlazorState;
  using Microsoft.JSInterop;

  /// <summary>
  /// Maintain the Route in Blazor-State
  /// </summary>
  internal class RouteState : State<RouteState>
  {
    public RouteState() { }

    protected RouteState(RouteState aState) : this()
    {
      Route = aState.Route;
    }

    public string Route { get; set; }

    public override object Clone() => new RouteState(this);

    public override RouteState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      return new RouteState
      {
        Guid = new System.Guid((string)aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))]),
        Route = (string)aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Route))]
      };
    }

    protected override void Initialize() { }
  }
}