// This test case is more an excersize of mocking.  End2End Test will validate Routing commands work.
//namespace TestApp.Client.Integration.Tests.Features.Routing
//{
//  using BlazorState;
//  using BlazorState.Features.Routing;
//  using MediatR;
//  using Microsoft.AspNetCore.Blazor.Hosting;
//  using Microsoft.AspNetCore.Components;
//  using Microsoft.Extensions.DependencyInjection;
//  using Microsoft.Extensions.DependencyInjection.Extensions;
//  using Moq;
//  using Shouldly;
//  using System;
//  using System.Threading.Tasks;
//  using TestApp.Client.Integration.Tests.Infrastructure;

//  internal class ChangeRouteTests
//  {
//    private const string NewUrl = "http://localhost:7169/";
//    private IMediator Mediator { get; }

//    private RouteState RouteState => Store.GetState<RouteState>();
//    private IServiceProvider ServiceProvider { get; }
//    private IStore Store { get; }

//    public ChangeRouteTests(TestFixture aTestFixture)
//    {
//      aTestFixture.WebAssemblyHostBuilder.ConfigureServices(ConfigureServices);

//      ServiceProvider = aTestFixture.ServiceProvider;
//      Mediator = ServiceProvider.GetService<IMediator>();
//      Store = ServiceProvider.GetService<IStore>();
//    }

//    /// <summary>
//    /// Not sure this test proves much.
//    /// The mock configuration is most important
//    /// </summary>
//    /// <returns></returns>
//    public async Task ShouldChangeRoute()
//    {
//      // Arrange
//      RouteState.Initialize("/someplace");
//      var changeRouteRequest = new ChangeRouteAction
//      {
//        NewRoute = NewUrl
//      };
//      // Act
//      await Mediator.Send(changeRouteRequest);

//      // Assert
//      RouteState.Route.ShouldBe(changeRouteRequest.NewRoute);
//    }

//    /// <summary>
//    /// Test specific Service Configuration
//    /// NavigationManager is used by the handler and would fail because we are not actually in the proper context when running this test
//    /// So we Mock the NavigationManager and replace the existing registration
//    /// </summary>
//    /// <param name="aServiceCollection"></param>
//    private void ConfigureServices(IServiceCollection aServiceCollection)
//    {
      
//      //var mock = new Mock<NavigationManager>();
//      //mock.Setup(aNavigationManager => aNavigationManager.ToAbsoluteUri(It.IsAny<string>())).Returns(new Uri(NewUrl));
//      //mock.Setup(aNavigationManager => aNavigationManager.Uri).Returns(NewUrl);
//      //mock.Setup(aNavigationManager => aNavigationManager.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()));

//      var descriptor = new ServiceDescriptor(typeof(NavigationManager), new TestNavigationManager());
//      aServiceCollection.Replace(descriptor);
//    }
//  }

//  internal class TestNavigationManager : NavigationManager
//  {
//    public TestNavigationManager() {
//      Initialize("http://localhost:7169/", "http://localhost:7169/original");
//    }

//    public new void Initialize(string aBaseUri, string aUri) => base.Initialize(aBaseUri, aUri);

//    protected override void NavigateToCore(string aUri, bool aForceLoad)
//    {
//      Uri = aUri;
//      // don't do anything
//      NotifyLocationChanged(true);
//    }
//  }
//}