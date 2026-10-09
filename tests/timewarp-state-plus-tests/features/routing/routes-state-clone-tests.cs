#region Purpose
// Checks RouteState's ICloneable.Clone: new instance and Guid, same Sender, same route URLs.
#endregion

#region Design
// Constructs RouteState with a FakeItEasy ISender and seeds two routes through the test-only Initialize.
#endregion

// ReSharper disable UnusedType.Global
namespace RouteState_;

public class Clone_Should
{
  public Clone_Should()
  {
    ISender<ClientPipeline> sender = A.Fake<ISender<ClientPipeline>>();
    RouteState = new RouteState(sender);
  }

  private RouteState RouteState { get; }

  public void Clone()
  {
    Stack<RouteState.RouteInfo> routeStack = new();
    routeStack.Push(new RouteState.RouteInfo("url1", "Title1"));
    routeStack.Push(new RouteState.RouteInfo("url2", "Title2"));

    RouteState.Initialize(routeStack);

    RouteState clone = (RouteState)((ICloneable)RouteState).Clone();

    clone.ShouldNotBeSameAs(RouteState);
    clone.Sender.ShouldBe(RouteState.Sender);
    clone.Routes.ShouldNotBeNull();
    clone.Guid.ShouldNotBe(RouteState.Guid);
    clone.Routes.Select(routeInfo => routeInfo.Url).ShouldBe(RouteState.Routes.Select(routeInfo => routeInfo.Url));
  }
}
