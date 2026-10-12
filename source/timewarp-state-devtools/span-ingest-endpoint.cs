#region Purpose
// Accepts POST /ingest/spans and appends captured spans to InspectorLog.
#endregion

#region Design
// Does not dispatch. The circuit hears InspectorLog.Changed and dispatches NoteLogChanged on its
// own store. InspectorCapture drops InspectorState here as well as in the exporter. The body is
// untrusted: JSON content type only (415), 1 MiB cap including chunked bodies (413), null-safe
// mapping, and snapshots capped at MaxSnapshotLength.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// HTTP ingest for the DevTools observation channel.
/// </summary>
public static class SpanIngestEndpoint
{
  /// <summary>Maximum spans accepted from one body.</summary>
  public const int MaxSpansPerRequest = 512;

  /// <summary>Maximum request body in bytes, enforced for chunked bodies too.</summary>
  public const int MaxBodyBytes = 1_048_576;

  /// <summary>Maximum stored snapshot length in characters. Longer snapshots are cut and marked truncated.</summary>
  public const int MaxSnapshotLength = 65_536;

  /// <summary>
  /// Maps <c>POST /ingest/spans</c>.
  /// </summary>
  public static void Map(WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);
    app.MapPost("/ingest/spans", IngestAsync).DisableAntiforgery();
  }

  internal static async Task<IResult> IngestAsync
  (
    HttpRequest request,
    InspectorLog inspectorLog,
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

    SpanIngestDocument? document;
    try
    {
      document = await JsonSerializer.DeserializeAsync
      (
        request.Body,
        SpanIngestJsonContext.Default.SpanIngestDocument,
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

    IReadOnlyList<SpanIngestItem?>? spans = document?.Spans;
    if (spans is null)
    {
      return Results.BadRequest();
    }

    int accepted = 0;
    int limit = Math.Min(spans.Count, MaxSpansPerRequest);
    for (int index = 0; index < limit; index++)
    {
      SpanIngestItem? item = spans[index];
      if (item is null || !TryMap(item, out ActionLogEntry? entry))
      {
        continue;
      }

      inspectorLog.Add(entry);
      accepted++;
    }

    return Results.Ok(accepted);
  }

  internal static bool TryMap(SpanIngestItem item, out ActionLogEntry entry)
  {
    ArgumentNullException.ThrowIfNull(item);
    string stateTypeName = Attribute(item, InspectorCapture.StateTypeTagName);
    string inspectorTag = Attribute(item, InspectorCapture.InspectorTagName);
    if (!InspectorCapture.ShouldCapture(stateTypeName, inspectorTag))
    {
      entry = null!;
      return false;
    }

    string actionName = Attribute(item, InspectorCapture.ActionTagName);
    if (actionName.Length == 0)
    {
      actionName = item.Name ?? "";
    }

    IReadOnlyDictionary<string, string> attributes = CopyAttributes(item);
    string kind = Attribute(item, InspectorCapture.KindTagName);
    if (kind.Length == 0 && string.Equals(actionName, InspectorCapture.InitKind, StringComparison.Ordinal))
    {
      kind = InspectorCapture.InitKind;
    }

    (string? snapshotJson, string? snapshotKind, bool snapshotTruncated) = LatestSnapshot(item);
    DateTime startTimeUtc = item.StartTimeUnixMilliseconds <= 0
      ? DateTime.UnixEpoch
      : DateTimeOffset.FromUnixTimeMilliseconds(item.StartTimeUnixMilliseconds).UtcDateTime;

    entry = new ActionLogEntry
    {
      Sequence = 0,
      Name = item.Name ?? "",
      ActionName = actionName,
      StateTypeName = stateTypeName,
      Status = item.Status ?? "",
      DurationMilliseconds = item.DurationMilliseconds,
      StartTimeUtc = startTimeUtc,
      SpanId = item.SpanId ?? "",
      SnapshotJson = snapshotJson,
      SnapshotKind = snapshotKind,
      SnapshotTruncated = snapshotTruncated,
      Kind = kind,
      Attributes = attributes
    };
    return true;
  }

  private static IReadOnlyDictionary<string, string> CopyAttributes(SpanIngestItem item)
  {
    if (item.Attributes is null || item.Attributes.Count == 0)
    {
      return ImmutableDictionary<string, string>.Empty;
    }

    ImmutableDictionary<string, string>.Builder builder =
      ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
    foreach (KeyValuePair<string, string> pair in item.Attributes)
    {
      if (pair.Value is null)
      {
        continue;
      }

      builder[pair.Key] = pair.Value;
    }

    return builder.ToImmutable();
  }

  private static string Attribute(SpanIngestItem item, string name)
  {
    if (item.Attributes is not null && item.Attributes.TryGetValue(name, out string? value) && value is not null)
    {
      return value;
    }

    return "";
  }

  private static (string? Json, string? Kind, bool Truncated) LatestSnapshot(SpanIngestItem item)
  {
    string? json = null;
    string? kind = null;
    bool truncated = false;
    foreach (SpanIngestEvent? spanEvent in item.Events ?? [])
    {
      if (spanEvent is null || spanEvent.Attributes is null)
      {
        continue;
      }

      if
      (
        !string.Equals(spanEvent.Name, InspectorCapture.SnapshotEventName, StringComparison.Ordinal)
        && !string.Equals(spanEvent.Name, InspectorCapture.DiffEventName, StringComparison.Ordinal)
      )
      {
        continue;
      }

      if (!spanEvent.Attributes.TryGetValue(InspectorCapture.SnapshotJsonTagName, out string? eventJson))
      {
        continue;
      }

      json = eventJson ?? "";
      kind = spanEvent.Name;
      truncated = spanEvent.Attributes.TryGetValue(InspectorCapture.SnapshotTruncatedTagName, out string? truncatedText)
        && string.Equals(truncatedText, "true", StringComparison.OrdinalIgnoreCase);
      if (json.Length > MaxSnapshotLength)
      {
        json = json[..MaxSnapshotLength];
        truncated = true;
      }
    }

    return (json, kind, truncated);
  }
}
