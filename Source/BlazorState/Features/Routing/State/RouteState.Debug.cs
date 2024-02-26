namespace BlazorState.Features.Routing;

public partial class RouteState
{
  public override RouteState Hydrate(IDictionary<string, object> keyValuePairs) => new()
  {
    Guid = 
      new Guid
      (
        keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString() ?? 
        throw new InvalidOperationException()
      ),
    Route = keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Route))].ToString()
  };

  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="route">The initial value for Route</param>
  public void Initialize(string route)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    Route = route;
  }
}
