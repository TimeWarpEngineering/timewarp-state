namespace TestApp.Server.Integration.Tests.Infrastructure
{
  using System;

  public class TestFixture
  {
    private readonly BlazorStateTestServer BlazorStateTestServer;

    /// <summary>
    /// This is the ServiceProvider that will be used by the Server
    /// </summary>
    public IServiceProvider ServiceProvider => BlazorStateTestServer.Services;

    public TestFixture(BlazorStateTestServer aBlazorStateTestServer)
    {
      BlazorStateTestServer = aBlazorStateTestServer;
    }
  }
}