#region Purpose
// Accepts POST /ingest/spans and appends captured spans to InspectorLog.
#endregion

#region Design
// Does not dispatch. The circuit hears InspectorLog.Changed and dispatches NoteLogChanged on its
// own store. InspectorCapture drops InspectorState here as well as in the exporter.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// HTTP ingest for the DevTools observation channel.
/// </summary>
public static class SpanIngestEndpoint
{
  /// <summary>Maximum spans accepted from one body.</summary>
  public const int MaxSpansPerRequest = 512;

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
    if (request.ContentLength is > 1_048_576)
    {
      return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
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

    if (document is null)
    {
      return Results.BadRequest();
    }

    int accepted = 0;
    int limit = Math.Min(document.Spans.Count, MaxSpansPerRequest);
    for (int index = 0; index < limit; index++)
    {
      SpanIngestItem item = document.Spans[index];
      if (!TryMap(item, out ActionLogEntry? entry))
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
      actionName = item.Name;
    }

    (string? snapshotJson, string? snapshotKind, bool snapshotTruncated) = LatestSnapshot(item);
    DateTime startTimeUtc = item.StartTimeUnixMilliseconds <= 0
      ? DateTime.UnixEpoch
      : DateTimeOffset.FromUnixTimeMilliseconds(item.StartTimeUnixMilliseconds).UtcDateTime;

    entry = new ActionLogEntry
    {
      Sequence = 0,
      Name = item.Name,
      ActionName = actionName,
      StateTypeName = stateTypeName,
      Status = item.Status,
      DurationMilliseconds = item.DurationMilliseconds,
      StartTimeUtc = startTimeUtc,
      SpanId = item.SpanId,
      SnapshotJson = snapshotJson,
      SnapshotKind = snapshotKind,
      SnapshotTruncated = snapshotTruncated
    };
    return true;
  }

  private static string Attribute(SpanIngestItem item, string name)
  {
    if (item.Attributes.TryGetValue(name, out string? value) && value is not null)
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
    foreach (SpanIngestEvent spanEvent in item.Events)
    {
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

      json = eventJson;
      kind = spanEvent.Name;
      truncated = spanEvent.Attributes.TryGetValue(InspectorCapture.SnapshotTruncatedTagName, out string? truncatedText)
        && string.Equals(truncatedText, "true", StringComparison.OrdinalIgnoreCase);
    }

    return (json, kind, truncated);
  }
}
