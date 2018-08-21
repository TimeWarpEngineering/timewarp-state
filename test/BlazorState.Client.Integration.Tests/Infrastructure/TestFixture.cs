namespace BlazorState.Integration.Tests.Infrastructure
{
  using System.Reflection;
  using BlazorState;
  using BlazorState.Client;
  using BlazorState.Client.Integration.Tests.Infrastructure;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;

  /// <summary>
  /// A known starting state(baseline) for all tests.
  /// And Common set of functions
  /// </summary>
  public class TestFixture//: IMediatorFixture, IStoreFixture, IServiceProviderFixture
  {
    /// <summary>
    /// This is the ServiceProvider that will be used by the Client
    /// </summary>
    public ServiceProvider ServiceProvider { get; set; }
    private IMediator Mediator { get; set; }
    private BlazorStateTestServer BlazorStateTestServer { get; }

    public TestFixture(BlazorStateTestServer aBlazorStateTestServer)
    {
      BlazorStateTestServer = aBlazorStateTestServer;

      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// This does what would be done in a StartUp class
    /// </summary>
    /// <param name="aServiceCollection"></param>
    private void ConfigureServices(ServiceCollection aServiceCollection)
    {
      aServiceCollection.AddSingleton(BlazorStateTestServer.CreateClient());
      aServiceCollection.AddBlazorState(null, Assembly.GetAssembly(typeof(Startup)));
    }
  }
}
