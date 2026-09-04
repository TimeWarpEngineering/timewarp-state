// ReSharper disable UnusedType.Global
namespace RouteState_;

using FakeItEasy;
using TimeWarp.Mediator;
using TimeWarp.State;
using TimeWarp.Features.Routing;

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
