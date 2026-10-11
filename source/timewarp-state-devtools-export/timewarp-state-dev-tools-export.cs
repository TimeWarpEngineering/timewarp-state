#region Purpose
// Registers the DevTools span exporter when the AppHost published an endpoint.
#endregion

#region Design
// No-op when TIMEWARP_STATE_DEVTOOLS_ENDPOINT is unset, so a production host that references this
// package by accident does not open a socket. The batch delay is short so the inspector stays
// close to the click. Protocol is the dev-only JSON document, not OTLP.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Registration for the DevTools observation channel.
/// </summary>
public static class TimeWarpStateDevToolsExport
{
  /// <summary>
  /// Environment variable set by <c>WithTimeWarpStateDevTools</c>. Value is the DevTools origin,
  /// for example <c>http://localhost:5288</c>. The exporter posts to <c>/ingest/spans</c> under it.
  /// </summary>
  public const string EndpointEnvironmentVariable = "TIMEWARP_STATE_DEVTOOLS_ENDPOINT";

  /// <summary>
  /// Adds a batch processor that copies <c>TimeWarp.State</c> activities to DevTools.
  /// Does nothing when <see cref="EndpointEnvironmentVariable"/> is unset or not an absolute URI.
  /// </summary>
  /// <param name="builder">The tracer provider builder.</param>
  /// <returns>The same builder.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
  public static TracerProviderBuilder AddTimeWarpStateDevToolsExporter(this TracerProviderBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    string? endpoint = Environment.GetEnvironmentVariable(EndpointEnvironmentVariable);
    if (string.IsNullOrWhiteSpace(endpoint))
    {
      return builder;
    }

    if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out Uri? baseUri))
    {
      throw new InvalidOperationException
      (
        $"{EndpointEnvironmentVariable} must be an absolute URI. Value: '{endpoint}'."
      );
    }

    Uri ingestUri = IngestUri(baseUri);
    DevToolsSpanExporter exporter = new(ingestUri);
    return builder.AddProcessor
    (
      new BatchActivityExportProcessor
      (
        exporter,
        scheduledDelayMilliseconds: 200
      )
    );
  }

  /// <summary>
  /// <c>{origin}/ingest/spans</c> for an absolute origin or base URI.
  /// </summary>
  /// <param name="baseUri">DevTools origin.</param>
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
}
