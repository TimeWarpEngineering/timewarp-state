namespace TestApp.Client.Integration.Tests.Infrastructure
{
  using Microsoft.AspNetCore.Blazor.Hosting;
  using Microsoft.Extensions.DependencyInjection;
  using System;
  using TestApp.Client.Features.CloneTest;

  /// <summary>
  /// A known starting state(baseline) for all tests.
  /// And Common set of functions
  /// </summary>
  public class TestFixture//: IMediatorFixture, IStoreFixture, IServiceProviderFixture
  {
    /// <summary>
    /// This is the ServiceProvider that will be used by the Client
    /// </summary>
    public IServiceProvider ServiceProvider => WebAssemblyHostBuilder.Build().Services;

    /// <summary>
    /// This is used to host the Client Side `TesatApp.Client`
    /// </summary>
    private readonly IWebAssemblyHostBuilder WebAssemblyHostBuilder;

    private readonly BlazorStateTestServer BlazorStateTestServer;

    public TestFixture(BlazorStateTestServer aBlazorStateTestServer)
    {
      BlazorStateTestServer = aBlazorStateTestServer;
      WebAssemblyHostBuilder = BlazorWebAssemblyHost.CreateDefaultBuilder()
        .ConfigureServices(ConfigureServices);
    }

    /// <summary>
    /// Special configuration for Testing with the Test Server
    /// </summary>
    /// <param name="aServiceCollection"></param>
    private void ConfigureServices(IServiceCollection aServiceCollection)
    {
      // The client needs to use the special HttpClient provided by the test Server.
      aServiceCollection.AddSingleton(BlazorStateTestServer.CreateClient());

      // Use the same configuration that we have in `TestApp.Client`
      new Client.Startup().ConfigureServices(aServiceCollection);
    }
  }
}