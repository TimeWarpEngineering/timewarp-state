namespace TestApp.Client.Integration.Tests.Features.Routing
{
  using System;
  using MediatR;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using Microsoft.Extensions.DependencyInjection;
  using BlazorState;
  using BlazorState.Features.Routing;
  using Shouldly;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Blazor.Hosting;
  using Moq;
  using Microsoft.AspNetCore.Components.Services;
  using Microsoft.Extensions.DependencyInjection.Extensions;

  internal class ChangeRouteTests
  {
    private IMediator Mediator { get; }

    private IServiceProvider ServiceProvider { get; }
    const string newUrl = "http://localhost:7169/";

    public ChangeRouteTests(TestFixture aTestFixture)
    {
      aTestFixture.WebAssemblyHostBuilder.ConfigureServices(ConfigureServices);

      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
    }
    /// <summary>
    /// Test specific Service Configuration
    /// </summary>
    /// <param name="aServiceCollection"></param>
    private void ConfigureServices(IServiceCollection aServiceCollection)
    {
      var mock = new Mock<IUriHelper>();
      mock.Setup(aUriHelper => aUriHelper.ToAbsoluteUri(It.IsAny<string>())).Returns(new Uri(newUrl));
      mock.Setup(aUriHelper => aUriHelper.GetAbsoluteUri()).Returns(newUrl);
      mock.Setup(aUriHelper => aUriHelper.NavigateTo(It.IsAny<string>()));

      var descriptor = new ServiceDescriptor(typeof(IUriHelper), mock.Object);
      aServiceCollection.Replace(descriptor);
    }


    private RouteState RouteState => Store.GetState<RouteState>();
    private IStore Store { get; }

    // TODO: IUrlHelper is used by the handler and fails because we are not actually in the proper context when running
    // Should moc the IUrlHelper and add to the Container

      /// <summary>
      /// Not sure this test proves much.
      /// The mock configuration is most important
      /// </summary>
      /// <returns></returns>
    public async Task ShouldChangeRoute()
    {
      // Arrange
      RouteState.Initialize("/someplace");
      var changeRouteRequest = new ChangeRouteRequest
      {
        NewRoute = newUrl
      };
      // Act
      await Mediator.Send(changeRouteRequest);

      // Assert
      RouteState.Route.ShouldBe(changeRouteRequest.NewRoute);
    }
  }

}
