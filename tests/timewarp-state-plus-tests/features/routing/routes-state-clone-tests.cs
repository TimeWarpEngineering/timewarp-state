// ReSharper disable UnusedType.Global
namespace RouteState_;

using AnyClone;
using FakeItEasy;
using TimeWarp.Mediator;
using TimeWarp.Features.Routing;

public class Clone_Should 
{
  public Clone_Should() 
  {
    ISender sender = A.Fake<ISender>();
    RouteState = new RouteState(sender);
  }

  private RouteState RouteState { get; }
  
  public void Clone()
  {
    //Arrange
    Stack<RouteState.RouteInfo> routeStack = new();
    routeStack.Push(new RouteState.RouteInfo("url1","Title1"));
    routeStack.Push(new RouteState.RouteInfo("url2","Title2"));

    RouteState.Initialize(routeStack);

    //Act
    RouteState clone = RouteState.Clone();

    //Assert
    clone.ShouldNotBeSameAs(RouteState);
    clone.Sender.ShouldBeNull(); // ISender is not cloned
    clone.Routes.ShouldNotBeNull();
    clone.Guid.ShouldNotBe(RouteState.Guid);
  }
}
