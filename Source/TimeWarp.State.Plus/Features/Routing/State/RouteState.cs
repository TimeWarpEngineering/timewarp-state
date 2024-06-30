namespace TimeWarp.Features.Routing;

using System.Text.Json.Serialization;

/// <summary>
/// Maintain the Route in TimeWarp.State
/// </summary>
public partial class RouteState : State<RouteState>
{
  private readonly Stack<RouteInfo> RouteStack = new();
  public RouteState(ISender sender) : base(sender) {}

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
