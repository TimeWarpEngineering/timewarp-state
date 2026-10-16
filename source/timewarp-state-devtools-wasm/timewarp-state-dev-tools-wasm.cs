#region Purpose
// Registers the browser DevTools exporter when an inspector origin is available in Development.
#endregion

#region Design
// Opt-in is the call, and a missing origin is a no-op: no listener and no socket. Outside
// Development the call returns without reading configuration or calling the discovery route.
// Resolution order is configuration TimeWarp:State:DevTools:Endpoint, then
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT (empty in the browser), then GET /_timewarp/state-devtools
// on the host base address. A value that is not an absolute http(s) URI is ignored. The server
// exporter throws on a bad URI because only the AppHost sets it. Here a bad value must not take
// the browser app down. The ingest URI is {origin}/ingest/spans, the same shape as the server exporter.
#endregion

namespace TimeWarp.State.DevTools.Wasm;

/// <summary>
/// Registration for the browser DevTools observation channel.
/// </summary>
public static class TimeWarpStateDevToolsWasm
{
  /// <summary>
  /// Configuration key for the DevTools origin. Same meaning as
  /// <see cref="EndpointEnvironmentVariable"/> when that variable is visible to the process.
  /// </summary>
  public const string EndpointConfigurationKey = "TimeWarp:State:DevTools:Endpoint";

  /// <summary>
  /// Same variable <c>WithTimeWarpStateDevTools</c> sets. The browser process does not receive it.
  /// A test asserts this text matches the server exporter's constant.
  /// </summary>
  public const string EndpointEnvironmentVariable = "TIMEWARP_STATE_DEVTOOLS_ENDPOINT";

  /// <summary>
  /// Same path <c>MapTimeWarpStateDevToolsClientConfig</c> maps. A test asserts the match.
  /// </summary>
  public const string DiscoveryPath = "/_timewarp/state-devtools";

  /// <summary>
  /// In Development, resolves an origin and registers <see cref="DevToolsWasmExporter"/>.
  /// Does nothing when the host is not Development or no origin is available.
  /// </summary>
  /// <param name="builder">The WebAssembly host builder.</param>
  /// <returns>The task that finishes registration.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
  public static async Task AddTimeWarpStateDevToolsWasmExporterAsync(this WebAssemblyHostBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    string? endpoint = await ResolveEndpointAsync
    (
      builder.HostEnvironment.IsDevelopment(),
      builder.Configuration[EndpointConfigurationKey],
      builder.HostEnvironment.BaseAddress
    ).ConfigureAwait(false);
    builder.Services.AddTimeWarpStateDevToolsWasmExporter(endpoint);
  }

  /// <summary>
  /// Registers <see cref="DevToolsWasmExporter"/> when <paramref name="endpoint"/> is an absolute
  /// http(s) origin. A null, blank, or non-absolute value registers nothing.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <param name="endpoint">DevTools origin, or null.</param>
  /// <returns>The same service collection.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> is null.</exception>
  public static IServiceCollection AddTimeWarpStateDevToolsWasmExporter
  (
    this IServiceCollection services,
    string? endpoint
  )
  {
    ArgumentNullException.ThrowIfNull(services);
    if (!TryCreateIngestUri(endpoint, out Uri? ingestUri))
    {
      return services;
    }

    services.AddSingleton(new DevToolsWasmExporter(ingestUri));
    return services;
  }

  /// <summary>
  /// Resolves a DevTools origin. Returns null when <paramref name="isDevelopment"/> is false,
  /// when no source has an origin, or when the value is not an absolute http(s) URI.
  /// Does not throw for a missing inspector or a bad value.
  /// </summary>
  /// <param name="isDevelopment">True when the host environment is Development.</param>
  /// <param name="configuredEndpoint">Value of <see cref="EndpointConfigurationKey"/>, or null.</param>
  /// <param name="baseAddress">Host base address used for discovery. Null skips discovery.</param>
  /// <param name="handler">Optional handler for the discovery request.</param>
  /// <param name="cancellationToken">Cancels discovery.</param>
  public static async Task<string?> ResolveEndpointAsync
  (
    bool isDevelopment,
    string? configuredEndpoint,
    string? baseAddress,
    HttpMessageHandler? handler = null,
    CancellationToken cancellationToken = default
  )
  {
    if (!isDevelopment)
    {
      return null;
    }

    if (string.IsNullOrWhiteSpace(configuredEndpoint))
    {
      configuredEndpoint = Environment.GetEnvironmentVariable(EndpointEnvironmentVariable);
    }

    if (string.IsNullOrWhiteSpace(configuredEndpoint) && !string.IsNullOrWhiteSpace(baseAddress))
    {
      configuredEndpoint = await DiscoverEndpointAsync(baseAddress, handler, cancellationToken)
        .ConfigureAwait(false);
    }

    if (!TryNormalizeOrigin(configuredEndpoint, out string? origin))
    {
      return null;
    }

    return origin;
  }

  /// <summary>
  /// GETs <see cref="DiscoveryPath"/> under <paramref name="baseAddress"/>.
  /// Returns the <c>endpoint</c> field, or null when the route is absent, empty, or fails.
  /// </summary>
  /// <param name="baseAddress">Absolute base address of the host.</param>
  /// <param name="handler">Optional handler. The caller keeps ownership.</param>
  /// <param name="cancellationToken">Cancels the GET.</param>
  public static async Task<string?> DiscoverEndpointAsync
  (
    string baseAddress,
    HttpMessageHandler? handler = null,
    CancellationToken cancellationToken = default
  )
  {
    if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out Uri? baseUri))
    {
      return null;
    }

    try
    {
      using HttpClient httpClient = handler is null
        ? new HttpClient()
        : new HttpClient(handler, disposeHandler: false);
      httpClient.Timeout = TimeSpan.FromSeconds(2);
      Uri uri = new(baseUri, DiscoveryPath);
      using HttpResponseMessage response = await httpClient.GetAsync(uri, cancellationToken)
        .ConfigureAwait(false);
      if (response.StatusCode == System.Net.HttpStatusCode.NoContent || !response.IsSuccessStatusCode)
      {
        return null;
      }

      string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
      using JsonDocument document = JsonDocument.Parse(body);
      if
      (
        !document.RootElement.TryGetProperty("endpoint", out JsonElement endpoint)
        || endpoint.ValueKind != JsonValueKind.String
      )
      {
        return null;
      }

      return endpoint.GetString();
    }
    catch (Exception exception) when (exception is not OutOfMemoryException)
    {
      return null;
    }
  }

  /// <summary>
  /// <c>{origin}/ingest/spans</c> for an absolute origin. Matches the server exporter.
  /// </summary>
  /// <param name="baseUri">DevTools origin.</param>
  /// <exception cref="ArgumentNullException"><paramref name="baseUri"/> is null.</exception>
  public static Uri IngestUri(Uri baseUri)
  {
    ArgumentNullException.ThrowIfNull(baseUri);
    string text = baseUri.AbsoluteUri;
    if (!text.EndsWith('/'))
    {
      text += "/";
    }

    return new Uri(new Uri(text, UriKind.Absolute), "ingest/spans");
  }

  private static bool TryCreateIngestUri(string? endpoint, out Uri ingestUri)
  {
    if (!TryNormalizeOrigin(endpoint, out string? origin))
    {
      ingestUri = null!;
      return false;
    }

    ingestUri = IngestUri(new Uri(origin, UriKind.Absolute));
    return true;
  }

  private static bool TryNormalizeOrigin(string? endpoint, out string origin)
  {
    origin = "";
    if (string.IsNullOrWhiteSpace(endpoint))
    {
      return false;
    }

    if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out Uri? uri))
    {
      return false;
    }

    if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
      && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    origin = uri.GetLeftPart(UriPartial.Authority);
    return origin.Length > 0;
  }
}
