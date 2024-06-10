namespace TimeWarp.Features.Routing;

/// <summary>
/// Maintain the Route in Blazor-State
/// </summary>
public partial class RouteState : State<RouteState>
{
  private readonly Stack<RouteInfo> HistoryStack = new();
  private bool GoingBack;
  public bool IsHistoryEmpty => HistoryStack.Count == 0;

  /// <summary>
  /// The collection of RouteInfo that have been navigated to
  /// </summary>
  /// <remarks>Is public so will be serialized and visible in DevTools and maybe UX wants to display the stack.</remarks>
  public IEnumerable<RouteInfo> History => HistoryStack;

  public string Route { get; private set; }

  public override void Initialize() {}
  
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
