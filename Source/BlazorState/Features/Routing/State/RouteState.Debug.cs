namespace BlazorState.Features.Routing
{
  using BlazorState;
  using Microsoft.JSInterop;
  using System.Collections.Generic;
  using System.Reflection;

  public partial class RouteState : State<RouteState>
  {
    public override RouteState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      return new RouteState
      {
        Guid = new System.Guid(aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString()),
        Route = aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Route))].ToString()
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