#region Purpose
// Development-only route a hosted Blazor app uses to tell the browser the inspector origin.
#endregion

#region Design
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT is process environment. The browser never sees it. The server
// that hosts the WASM app maps GET /_timewarp/state-devtools only in Development and returns that
// origin. Outside Development the route is not mapped. An unset variable is 204, not an empty
// origin, so the client stays idle. The body is camelCase JSON, separate from the span ingest
// document. A bad or missing value does not throw: the client treats that as opt-out.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// JSON body of <see cref="TimeWarpStateDevToolsClientConfigEndpoint.Path"/>.
/// </summary>
public sealed class TimeWarpStateDevToolsClientConfig
{
  /// <summary>DevTools origin, for example <c>http://localhost:5288</c>.</summary>
  public string Endpoint { get; set; } = "";
}

/// <summary>
/// Source-generated JSON contract for <see cref="TimeWarpStateDevToolsClientConfig"/>.
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TimeWarpStateDevToolsClientConfig))]
public sealed partial class TimeWarpStateDevToolsClientConfigJsonContext : JsonSerializerContext;

/// <summary>
/// Maps the Development-only discovery route for the WASM exporter.
/// </summary>
public static class TimeWarpStateDevToolsClientConfigEndpoint
{
  /// <summary>Relative path of the discovery route. Same-origin with the app that hosts WASM.</summary>
  public const string Path = "/_timewarp/state-devtools";

  /// <summary>
  /// Maps <see cref="Path"/> when <paramref name="app"/> is running in Development.
  /// Does nothing in every other environment.
  /// </summary>
  /// <param name="app">The inspected app.</param>
  /// <returns>The same application.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="app"/> is null.</exception>
  public static WebApplication MapTimeWarpStateDevToolsClientConfig(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);
    if (!app.Environment.IsDevelopment())
    {
      return app;
    }

    app.MapGet(Path, () =>
    {
      string? endpoint = Environment.GetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
      if (string.IsNullOrWhiteSpace(endpoint))
      {
        return Results.NoContent();
      }

      TimeWarpStateDevToolsClientConfig body = new()
      {
        Endpoint = endpoint.Trim()
      };
      return Results.Json(body, TimeWarpStateDevToolsClientConfigJsonContext.Default.TimeWarpStateDevToolsClientConfig);
    });

    return app;
  }
}
