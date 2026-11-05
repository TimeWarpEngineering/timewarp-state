#region Purpose
// Accepts POST /ingest/manifest and stores each valid State manifest in InspectorManifest.
#endregion

#region Design
// Mirrors SpanIngestEndpoint. The body is untrusted: JSON content type only (415), 1 MiB cap including chunked
// bodies (413), malformed JSON or a missing list is 400. Each entry is manifest JSON text parsed with
// StateManifest.Parse; an entry that is null, too long, or not a manifest is skipped, not an error, so one bad
// assembly does not hide the others. The response body is the number of manifests stored. CORS uses the same
// policy as the span route.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// HTTP ingest for the inspected app's State manifests.
/// </summary>
public static class ManifestIngestEndpoint
{
  /// <summary>Route that accepts manifests. CORS is required on this route only.</summary>
  public const string Route = "/ingest/manifest";

  /// <summary>Maximum manifests read from one body.</summary>
  public const int MaxManifestsPerRequest = 32;

  /// <summary>Maximum request body in bytes, enforced for chunked bodies too.</summary>
  public const int MaxBodyBytes = 1_048_576;

  /// <summary>Maximum length in characters of one manifest document. Longer entries are skipped.</summary>
  public const int MaxManifestLength = 524_288;

  /// <summary>
  /// Maps <c>POST /ingest/manifest</c>. When <paramref name="requireCors"/> is true, the route
  /// requires <see cref="DevToolsIngestCors.PolicyName"/>.
  /// </summary>
  /// <param name="app">The inspector application.</param>
  /// <param name="requireCors">True when <see cref="DevToolsIngestCors.Add"/> registered the policy.</param>
  public static void Map(WebApplication app, bool requireCors = false)
  {
    ArgumentNullException.ThrowIfNull(app);
    RouteHandlerBuilder route = app.MapPost(Route, IngestAsync).DisableAntiforgery();
    if (requireCors)
    {
      route.RequireCors(DevToolsIngestCors.PolicyName);
    }
  }

  internal static async Task<IResult> IngestAsync
  (
    HttpRequest request,
    InspectorManifest inspectorManifest,
    CancellationToken cancellationToken
  )
  {
    if (!request.HasJsonContentType())
    {
      return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
    }

    if (request.ContentLength is > MaxBodyBytes)
    {
      return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    IHttpMaxRequestBodySizeFeature? sizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (sizeFeature is { IsReadOnly: false })
    {
      sizeFeature.MaxRequestBodySize = MaxBodyBytes;
    }

    ManifestIngestDocument? document;
    try
    {
      document = await JsonSerializer.DeserializeAsync
      (
        request.Body,
        ManifestIngestJsonContext.Default.ManifestIngestDocument,
        cancellationToken
      );
    }
    catch (JsonException)
    {
      return Results.BadRequest();
    }
    catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
    {
      return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
    }

    IReadOnlyList<string?>? manifests = document?.Manifests;
    if (manifests is null)
    {
      return Results.BadRequest();
    }

    List<StateManifest> parsed = [];
    int limit = Math.Min(manifests.Count, MaxManifestsPerRequest);
    for (int index = 0; index < limit; index++)
    {
      if (TryParse(manifests[index], out StateManifest? manifest))
      {
        parsed.Add(manifest);
      }
    }

    // One store call per request, so the inspector raises Changed once rather than once per manifest.
    return Results.Ok(inspectorManifest.SetAll(parsed));
  }

  internal static bool TryParse(string? json, [NotNullWhen(true)] out StateManifest? manifest)
  {
    manifest = null;
    if (string.IsNullOrWhiteSpace(json) || json.Length > MaxManifestLength)
    {
      return false;
    }

    try
    {
      manifest = StateManifest.Parse(json);
      return true;
    }
    catch (Exception exception) when (exception is JsonException or FormatException or InvalidOperationException)
    {
      return false;
    }
  }
}
