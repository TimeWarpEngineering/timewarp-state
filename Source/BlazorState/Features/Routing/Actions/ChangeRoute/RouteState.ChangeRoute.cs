namespace BlazorState.Features.Routing;

public partial class RouteState
{
  public static class ChangeRoute
  {
    public class Action : IAction
    {
      public string NewRoute { get; init; }
    }
  }
}
