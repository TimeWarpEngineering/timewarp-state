namespace TestApp.Client.Integration.Tests.Infrastructure
{
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.TestHost;


  /// <summary>
  /// This is a test version of the `TestApp.Server`.
  /// The `TestFixture` class will use the HttpClient from this class
  /// </summary>
  public class BlazorStateTestServer : TestServer
  {
    public BlazorStateTestServer() : base(WebHostBuilder()) { }

    private static IWebHostBuilder WebHostBuilder() =>
      WebHost.CreateDefaultBuilder()
      .UseStartup<Server.Startup>()
      .UseEnvironment("Local");
  }
}