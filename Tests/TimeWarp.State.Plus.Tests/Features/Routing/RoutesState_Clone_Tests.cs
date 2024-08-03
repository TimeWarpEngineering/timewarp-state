// ReSharper disable UnusedType.Global
namespace RouteState_;

using AnyClone;
using FakeItEasy;
using MediatR;
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
    clone.Should().NotBeSameAs(RouteState);
    clone.Sender.Should().BeNull(); // ISender is not cloned
    clone.Routes.Should().NotBeNull();
    clone.Guid.Should().NotBe(RouteState.Guid);
  }
}
