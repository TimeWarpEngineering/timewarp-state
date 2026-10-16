#region Purpose
// Proves ingest CORS allows a configured Development origin and refuses everything else.
#endregion

#region Design
// The policy is read when the inspector host starts. Tests set TimeWarp:State:DevTools:CorsOrigins
// and clear TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS so the machine environment cannot widen the policy.
// A browser preflight from an allowed origin receives Access-Control-Allow-Private-Network. Production
// and an empty origin list register no policy, so a cross-origin browser post is not accepted.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class DevToolsIngestCors_Should
{
  private const string AllowedOrigin = "https://wasm.example:8443";

  public static async Task AllowAConfiguredDevelopmentOrigin()
  {
    await WithOrigins(AllowedOrigin + "/counter", null, async factory =>
    {
      HttpResponseMessage response = await PostAsync(factory, AllowedOrigin);
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      response.Headers.GetValues("Access-Control-Allow-Origin").ShouldContain(AllowedOrigin);
    });
  }

  public static async Task DenyAnOriginThatWasNotConfigured()
  {
    await WithOrigins(AllowedOrigin, null, async factory =>
    {
      HttpResponseMessage response = await PostAsync(factory, "https://evil.example");
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      Allows(response, "https://evil.example").ShouldBeFalse();
    });
  }

  public static async Task AllowNothingWhenNoOriginIsConfigured()
  {
    await WithOrigins(null, null, async factory =>
    {
      HttpResponseMessage response = await PostAsync(factory, AllowedOrigin);
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      Allows(response, AllowedOrigin).ShouldBeFalse();
    });
  }

  public static async Task AllowNothingInProductionEvenWhenOriginsAreConfigured()
  {
    await WithOrigins(AllowedOrigin, Environments.Production, async factory =>
    {
      HttpResponseMessage response = await PostAsync(factory, AllowedOrigin);
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
      Allows(response, AllowedOrigin).ShouldBeFalse();
    });
  }

  public static async Task AnswerAPrivateNetworkPreflightForAnAllowedOrigin()
  {
    await WithOrigins(AllowedOrigin, null, async factory =>
    {
      using HttpRequestMessage request = new(HttpMethod.Options, "/ingest/spans");
      request.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);
      request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
      request.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "content-type");
      request.Headers.TryAddWithoutValidation("Access-Control-Request-Private-Network", "true");
      HttpResponseMessage response = await factory.CreateClient().SendAsync(request);
      response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
      response.Headers.GetValues("Access-Control-Allow-Origin").ShouldContain(AllowedOrigin);
      response.Headers.GetValues("Access-Control-Allow-Private-Network").ShouldContain("true");
    });
  }

  public static async Task OmitPrivateNetworkAccessWhenThePolicyIsOff()
  {
    await WithOrigins(null, null, async factory =>
    {
      using HttpRequestMessage request = new(HttpMethod.Options, "/ingest/spans");
      request.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);
      request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
      request.Headers.TryAddWithoutValidation("Access-Control-Request-Private-Network", "true");
      HttpResponseMessage response = await factory.CreateClient().SendAsync(request);
      response.Headers.Contains("Access-Control-Allow-Private-Network").ShouldBeFalse();
      Allows(response, AllowedOrigin).ShouldBeFalse();
    });
  }

  public static async Task AcceptAPostThatSendsNoOrigin()
  {
    await WithOrigins(AllowedOrigin, null, async factory =>
    {
      using StringContent content = new("""{"Spans":[]}""", Encoding.UTF8, "application/json");
      HttpResponseMessage response = await factory.CreateClient().PostAsync("/ingest/spans", content);
      response.StatusCode.ShouldBe(HttpStatusCode.OK);
    });
  }

  private static bool Allows(HttpResponseMessage response, string origin)
  {
    return response.Headers.TryGetValues("Access-Control-Allow-Origin", out IEnumerable<string>? values)
      && values.Contains(origin, StringComparer.Ordinal);
  }

  private static async Task<HttpResponseMessage> PostAsync(WebApplicationFactory<Program> factory, string origin)
  {
    using HttpRequestMessage request = new(HttpMethod.Post, "/ingest/spans")
    {
      Content = new StringContent("""{"Spans":[]}""", Encoding.UTF8, "application/json")
    };
    request.Headers.TryAddWithoutValidation("Origin", origin);
    return await factory.CreateClient().SendAsync(request);
  }

  private static async Task WithOrigins
  (
    string? origins,
    string? environment,
    Func<WebApplicationFactory<Program>, Task> action
  )
  {
    string? previous = Environment.GetEnvironmentVariable(DevToolsIngestCors.OriginsEnvironmentVariable);
    Environment.SetEnvironmentVariable(DevToolsIngestCors.OriginsEnvironmentVariable, null);
    try
    {
      await using WebApplicationFactory<Program> factory = new();
      WebApplicationFactory<Program> configured = factory.WithWebHostBuilder(builder =>
      {
        if (environment is not null)
        {
          builder.UseEnvironment(environment);
        }

        if (origins is not null)
        {
          builder.UseSetting(DevToolsIngestCors.OriginsConfigurationKey, origins);
        }
      });
      await using (configured)
      {
        await action(configured);
      }
    }
    finally
    {
      Environment.SetEnvironmentVariable(DevToolsIngestCors.OriginsEnvironmentVariable, previous);
    }
  }
}
