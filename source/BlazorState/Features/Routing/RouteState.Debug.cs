namespace BlazorState.Features.Routing
{
  using System.Collections.Generic;
  using BlazorState;
  using Microsoft.JSInterop;

  public partial class RouteState : State<RouteState>
  {
    public override RouteState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      return new RouteState
      {
        Guid = new System.Guid((string)aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))]),
        Route = (string)aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Route))]
      };
    }
  }
}