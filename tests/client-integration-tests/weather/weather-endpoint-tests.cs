#region Purpose
// Proves the test app server's weather endpoint answers a plain api/weather request, with Days defaulting to 5.
#endregion

#region Design
// Resolves the HttpClient the TestingConvention points at the in-process Test.App.Server and GETs the route template
// with no query string. Without the default, the required Days parameter made this a 400.
#endregion

namespace WeatherEndpointTests;

public class Endpoint_Should
{
  private readonly HttpClient HttpClient;

  public Endpoint_Should(ClientHost clientHost)
  {
    HttpClient = clientHost.ServiceProvider.GetRequiredService<HttpClient>();
  }

  public async Task Return_Five_Forecasts_When_Days_Is_Omitted()
  {
    using HttpResponseMessage response = await HttpClient.GetAsync(Query.RouteTemplate);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    body.RootElement.ValueKind.ShouldBe(JsonValueKind.Array);
    body.RootElement.GetArrayLength().ShouldBe(5);
  }
}
