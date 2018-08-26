namespace BlazorState.Integration.Tests.Infrastructure
{
  using System;
  using System.Reflection;
  using BlazorState;
  using BlazorState.Client;
  using BlazorState.Client.Integration.Tests.Infrastructure;
  using Microsoft.AspNetCore.Blazor.Hosting;
  using Microsoft.Extensions.DependencyInjection;

  /// <summary>
  /// A known starting state(baseline) for all tests.
  /// And Common set of functions
  /// </summary>
  public class TestFixture//: IMediatorFixture, IStoreFixture, IServiceProviderFixture
  {
    public TestFixture(BlazorStateTestServer aBlazorStateTestServer)
    {
      BlazorStateTestServer = aBlazorStateTestServer;
      IWebAssemblyHostBuilder webAssemblyHostBuilder =
        BlazorWebAssemblyHost.CreateDefaultBuilder()
          //.UseBlazorStartup<Startup>()
          .ConfigureServices(ConfigureServices);

      ServiceProvider = webAssemblyHostBuilder.Build().Services;
    }

    /// <summary>
    /// This is the ServiceProvider that will be used by the Client
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    private BlazorStateTestServer BlazorStateTestServer { get; }

    /// <summary>
    /// Special configuration for Testing with the Test Server
    /// </summary>
    /// <param name="aServiceCollection"></param>
    private void ConfigureServices(IServiceCollection aServiceCollection)
    {
      aServiceCollection.AddSingleton(BlazorStateTestServer.CreateClient());
      aServiceCollection.AddBlazorState(null, typeof(Startup).GetTypeInfo().Assembly);
    }
  }
}