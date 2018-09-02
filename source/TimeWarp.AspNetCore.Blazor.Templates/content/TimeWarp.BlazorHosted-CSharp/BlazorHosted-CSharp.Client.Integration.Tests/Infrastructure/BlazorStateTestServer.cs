namespace BlazorHosted_CSharp.Client.Integration.Tests.Infrastructure
{
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.TestHost;

  public class BlazorStateTestServer : TestServer
  {
    public BlazorStateTestServer() : base(WebHostBuilder()) { }

    private static IWebHostBuilder WebHostBuilder() =>
      WebHost.CreateDefaultBuilder()
      .UseStartup<Server.Startup>()
      .UseEnvironment("Local");
  }
}