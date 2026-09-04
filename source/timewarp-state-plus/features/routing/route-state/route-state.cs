namespace TimeWarp.Features.Routing;

/// <summary>
/// Maintain the Route in TimeWarp.State
/// </summary>
public sealed partial class RouteState : State<RouteState>, ICloneable
{
  private readonly Stack<RouteInfo> RouteStack = new();
  public RouteState(ISender<ClientPipeline> sender) : base(sender) {}

  [JsonConstructor]
  public RouteState() {}

  private bool IsRouteStackEmpty => RouteStack.Count == 0;
  public bool CanGoBack => RouteStack.Count > 1;

  /// <summary>
  /// The collection of RouteInfo that have been navigated to
  /// </summary>
  /// <remarks>Is public so will be serialized and visible in DevTools and maybe UX wants to display the stack.</remarks>
  public IEnumerable<RouteInfo> Routes => RouteStack;

  public override void Initialize()
  {
    RouteStack.Clear();
  }

  /// <summary>
  /// Copy the route stack without AnyClone. Stack is mutated on navigation; AnyClone throws
  /// CloneException (ILCacheKey / concurrent update) and crashes the Blazor circuit.
  /// </summary>
  public object Clone()
  {
    var clonedState = new RouteState(Sender);
    foreach (RouteInfo routeInfo in RouteStack.Reverse())
    {
      clonedState.RouteStack.Push(routeInfo);
    }

    return clonedState;
  }
  
  internal void Initialize(Stack<RouteInfo> routeStack)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    RouteStack.Clear();
    foreach (RouteInfo routeInfo in routeStack)
    {
      RouteStack.Push(routeInfo);
    }
  }

  public class RouteInfo
  {
    public string Url { get; }
    public string PageTitle { get; }

    public RouteInfo(string url, string pageTitle)
    {
      Url = url;
      PageTitle = pageTitle;
    }

    public override string ToString() => PageTitle;
  }
}
