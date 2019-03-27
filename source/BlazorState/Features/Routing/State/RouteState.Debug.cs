namespace BlazorState.Features.Routing
{
  using System.Collections.Generic;
  using System.Reflection;
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

    /// <summary>
    /// Use in Tests ONLY, to initialize the State
    /// </summary>
    /// <param name="aRoute">The initial value for Route</param>
    public void Initialize(string aRoute)
    {
      ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
      Route = aRoute;
    }
  }
}