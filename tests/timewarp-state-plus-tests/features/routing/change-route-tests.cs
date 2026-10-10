#region Purpose
// RouteState.ChangeRoute navigates only when the absolute URI differs from the current one.
#endregion

#region Design
// A NavigationManager records NavigateToCore. The handler is called directly. No browser.
#endregion

namespace ChangeRoute_;

using Microsoft.Extensions.Logging.Abstractions;

public class ChangeRoute_Should
{
  public async Task Navigate_When_The_Route_Differs()
  {
    RecordingNavigationManager navigationManager = new("http://localhost/current");
    RouteState.ChangeRouteActionSet.Handler handler = Create(navigationManager);

    await handler.Handle(new RouteState.ChangeRouteActionSet.Action("/next"), CancellationToken.None);

    navigationManager.Navigated.ShouldBe(["http://localhost/next"]);
  }

  public async Task Skip_Navigation_When_The_Route_Is_Already_Current()
  {
    RecordingNavigationManager navigationManager = new("http://localhost/current");
    RouteState.ChangeRouteActionSet.Handler handler = Create(navigationManager);

    await handler.Handle(new RouteState.ChangeRouteActionSet.Action("/current"), CancellationToken.None);

    navigationManager.Navigated.ShouldBeEmpty();
  }

  private static RouteState.ChangeRouteActionSet.Handler Create(RecordingNavigationManager navigationManager)
  {
    IStore store = A.Fake<IStore>();
    return new RouteState.ChangeRouteActionSet.Handler
    (
      store,
      navigationManager,
      NullLogger<RouteState.ChangeRouteActionSet.Handler>.Instance
    );
  }

  private sealed class RecordingNavigationManager : NavigationManager
  {
    public List<string> Navigated { get; } = [];

    public RecordingNavigationManager(string uri) => Initialize("http://localhost/", uri);

    protected override void NavigateToCore(string uri, bool forceLoad) => Navigated.Add(uri);
  }
}
