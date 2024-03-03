namespace TimeWarp.Features.Routing;

/// <summary>
/// Maintain the Route in Blazor-State
/// </summary>
public partial class RouteState : State<RouteState>
{
  private readonly Stack<string> HistoryStack = new();
  private bool GoingBack;
  public bool IsHistoryEmpty => HistoryStack.Count == 0;

  /// <summary>
  /// The collection of routes that have been navigated to
  /// </summary>
  /// <remarks>Is public so will be serialized and visible in DevTools and maybe UX wants to display the stack.</remarks>
  // ReSharper disable once MemberCanBePrivate.Global
  public IReadOnlyCollection<string> History => HistoryStack;

  public string Route { get; private set; }

  public override void Initialize() {}
}
