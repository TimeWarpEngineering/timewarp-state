#region Purpose
// Proves the hosted app tells a Development browser the inspector origin and hides that route otherwise.
#endregion

#region Design
// The route lives on the inspected server, not on the inspector. These tests start a TestServer host
// that only maps MapTimeWarpStateDevToolsClientConfig. Development with the env var returns camelCase
// JSON. Development without it returns 204. Production does not map the route.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class TimeWarpStateDevToolsClientConfigEndpoint_Should
{
  public static async Task ReturnTheTrimmedOriginInDevelopment()
  {
    await WithEndpoint("  http://localhost:5288  ", Environments.Development, async client =>
    {
      HttpResponseMessage response = await client.GetAsync(TimeWarpStateDevToolsClientConfigEndpoint.Path);
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      string body = await response.Content.ReadAsStringAsync();
      body.ShouldContain("\"endpoint\":\"http://localhost:5288\"");
    });
  }

  public static async Task ReturnNoContentWhenTheEndpointIsUnset()
  {
    await WithEndpoint(null, Environments.Development, async client =>
    {
      HttpResponseMessage response = await client.GetAsync(TimeWarpStateDevToolsClientConfigEndpoint.Path);
      response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    });
  }

  public static async Task LeaveTheRouteUnmappedOutsideDevelopment()
  {
    await WithEndpoint("http://localhost:5288", Environments.Production, async client =>
    {
      HttpResponseMessage response = await client.GetAsync(TimeWarpStateDevToolsClientConfigEndpoint.Path);
      response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    });
  }

  private static async Task WithEndpoint(string? value, string environmentName, Func<HttpClient, Task> action)
  {
    string? previous = Environment.GetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
    Environment.SetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable, value);
    try
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder
      (
        new WebApplicationOptions
        {
          ApplicationName = "TimeWarp.State.DevTools.Tests",
          EnvironmentName = environmentName
        }
      );
      builder.WebHost.UseTestServer();
      WebApplication app = builder.Build();
      app.MapTimeWarpStateDevToolsClientConfig();
      await app.StartAsync();
      try
      {
        await action(app.GetTestClient());
      }
      finally
      {
        await app.StopAsync();
        await app.DisposeAsync();
      }
    }
    finally
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable, previous);
    }
  }
}
