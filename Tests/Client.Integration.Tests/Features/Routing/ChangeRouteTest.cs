namespace TestApp.Client.Integration.Tests.Features.Routing
{
  using System;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorState.Features.Routing;
  using MediatR;
  using Microsoft.AspNetCore.Blazor.Hosting;
  using Microsoft.AspNetCore.Components;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.DependencyInjection.Extensions;
  using Moq;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class ChangeRouteTests
  {
    private const string NewUrl = "http://localhost:7169/";
    private IMediator Mediator { get; }

    private RouteState RouteState => Store.GetState<RouteState>();
    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }

    public ChangeRouteTests(TestFixture aTestFixture)
    {
      aTestFixture.WebAssemblyHostBuilder.ConfigureServices(ConfigureServices);

      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
    }

    /// <summary>
    /// Not sure this test proves much.
    /// The mock configuration is most important
    /// </summary>
    /// <returns></returns>
    public async Task ShouldChangeRoute()
    {
      // Arrange
      RouteState.Initialize("/someplace");
      var changeRouteRequest = new ChangeRouteAction
      {
        NewRoute = NewUrl
      };
      // Act
      await Mediator.Send(changeRouteRequest);

      // Assert
      RouteState.Route.ShouldBe(changeRouteRequest.NewRoute);
    }

    /// <summary>
    /// Test specific Service Configuration
    /// IUrlHelper is used by the handler and would fail because we are not actually in the proper context when running this test
    /// So we Mock the IUrlHelper and replace the existing registration
    /// </summary>
    /// <param name="aServiceCollection"></param>
    private void ConfigureServices(IServiceCollection aServiceCollection)
    {
      var mock = new Mock<IUriHelper>();
      mock.Setup(aUriHelper => aUriHelper.ToAbsoluteUri(It.IsAny<string>())).Returns(new Uri(NewUrl));
      mock.Setup(aUriHelper => aUriHelper.GetAbsoluteUri()).Returns(NewUrl);
      mock.Setup(aUriHelper => aUriHelper.NavigateTo(It.IsAny<string>()));

      var descriptor = new ServiceDescriptor(typeof(IUriHelper), mock.Object);
      aServiceCollection.Replace(descriptor);
    }
  }
}