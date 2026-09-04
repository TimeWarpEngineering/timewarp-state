// Temporary repro test for code-review finding 3 (GoBack off-by-one).
// ReSharper disable UnusedType.Global
namespace GoBackRepro_;

using FakeItEasy;
using Microsoft.AspNetCore.Components;
using TimeWarp.Mediator;
using TimeWarp.Features.Routing;
using TimeWarp.State;

public class GoBack_Should
{
  private sealed class RecordingNavigationManager : NavigationManager
  {
    public readonly List<string> Navigations = [];
    public RecordingNavigationManager() => Initialize("http://localhost/", "http://localhost/current");
    protected override void NavigateToCore(string uri, bool forceLoad) => Navigations.Add(uri);
  }

  private static (RouteState.GoBackActionSet.Handler Handler, RecordingNavigationManager Nav, RouteState RouteState)
    CreateHandler(params RouteState.RouteInfo[] stackBottomToTop)
  {
    ISender<ClientPipeline> sender = A.Fake<ISender<ClientPipeline>>();
    var routeState = new RouteState(sender);

    Stack<RouteState.RouteInfo> seed = new();
    // RouteState.Initialize(Stack) re-pushes in enumeration order (top-first),
    // which reverses the stack — so push bottom-to-top here to end up bottom-to-top after the copy...
    // To be safe, push in reverse so the LAST element of stackBottomToTop ends up on top.
    foreach (RouteState.RouteInfo routeInfo in stackBottomToTop)
    {
      seed.Push(routeInfo);
    }

    // seed enumerates top-first; Initialize pushes in that order, so the original bottom ends up on top.
    // For this repro only the COUNT matters for the crash, order only matters for the navigation-target assertion.
    routeState.Initialize(seed);

    IStore store = A.Fake<IStore>();
    A.CallTo(() => store.GetState<RouteState>()).Returns(routeState);

    var nav = new RecordingNavigationManager();
    var handler = new RouteState.GoBackActionSet.Handler(store, nav);
    return (handler, nav, routeState);
  }

  public async Task Not_Throw_When_Amount_Equals_Stack_Depth_1()
  {
    (RouteState.GoBackActionSet.Handler handler, RecordingNavigationManager nav, _) =
      CreateHandler(new RouteState.RouteInfo("url1", "Title1"));

    // Stack depth 1 (only the current page, no history): GoBack is a no-op, not a crash.
    await handler.Handle(new RouteState.GoBackActionSet.Action(), CancellationToken.None);

    nav.Navigations.ShouldBeEmpty();
  }

  public async Task Not_Throw_When_Amount_Equals_Stack_Depth_2()
  {
    (RouteState.GoBackActionSet.Handler handler, RecordingNavigationManager nav, _) =
      CreateHandler(new RouteState.RouteInfo("url1", "Title1"), new RouteState.RouteInfo("url2", "Title2"));

    await handler.Handle(new RouteState.GoBackActionSet.Action(amount: 2), CancellationToken.None);

    nav.Navigations.ShouldNotBeEmpty();
  }

  public async Task Navigate_Back_One_When_Stack_Has_Two()
  {
    // Sanity check that the normal case works: depth 2, go back 1.
    (RouteState.GoBackActionSet.Handler handler, RecordingNavigationManager nav, _) =
      CreateHandler(new RouteState.RouteInfo("url1", "Title1"), new RouteState.RouteInfo("url2", "Title2"));

    await handler.Handle(new RouteState.GoBackActionSet.Action(), CancellationToken.None);

    nav.Navigations.Count.ShouldBe(1);
  }
}
