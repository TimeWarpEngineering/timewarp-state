#region Purpose
// Posts completed TimeWarp.State activities to the DevTools ingest endpoint.
#endregion

#region Design
// BaseExporter.Export is synchronous; the batch processor calls it off the request thread.
// HttpClient.Send stays on that thread. Failures return Failure and do not throw, so a stopped
// inspector cannot fail the inspected app. Only ActivitySource TimeWarp.State is copied, and
// InspectorCapture drops the inspector's own spans before the POST.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Exports <see cref="Activity"/> batches to <c>POST /ingest/spans</c>.
/// </summary>
public sealed class DevToolsSpanExporter : BaseExporter<Activity>
{
  private readonly HttpClient HttpClient;
  private readonly Uri IngestUri;

  /// <summary>
  /// Creates an exporter that posts to <paramref name="ingestUri"/>.
  /// </summary>
  /// <param name="ingestUri">Absolute <c>/ingest/spans</c> URI.</param>
  public DevToolsSpanExporter(Uri ingestUri)
  {
    ArgumentNullException.ThrowIfNull(ingestUri);
    if (!ingestUri.IsAbsoluteUri)
    {
      throw new ArgumentException("Ingest URI must be absolute.", nameof(ingestUri));
    }

    IngestUri = ingestUri;
    HttpClient = new HttpClient
    {
      Timeout = TimeSpan.FromSeconds(2)
    };
  }

  /// <inheritdoc />
  public override ExportResult Export(in Batch<Activity> batch)
  {
    List<Activity> activities = [];
    foreach (Activity activity in batch)
    {
      activities.Add(activity);
    }

    return ExportActivities(activities);
  }

  /// <summary>
  /// Builds the JSON document for <paramref name="activity"/>, or null when the span is not captured.
  /// </summary>
  internal static SpanIngestItem? BuildItem(Activity activity)
  {
    ArgumentNullException.ThrowIfNull(activity);
    if (!ShouldExport(activity))
    {
      return null;
    }

    SpanIngestItem item = new()
    {
      Name = activity.DisplayName ?? activity.OperationName,
      TraceId = activity.TraceId.ToString(),
      SpanId = activity.SpanId.ToString(),
      Status = activity.Status.ToString(),
      StartTimeUnixMilliseconds = new DateTimeOffset(activity.StartTimeUtc).ToUnixTimeMilliseconds(),
      DurationMilliseconds = activity.Duration.TotalMilliseconds,
      Attributes = StringTags(activity.TagObjects)
    };

    foreach (ActivityEvent activityEvent in activity.Events)
    {
      item.Events.Add
      (
        new SpanIngestEvent
        {
          Name = activityEvent.Name,
          Attributes = StringTags(activityEvent.Tags)
        }
      );
    }

    return item;
  }

  internal static bool ShouldExport(Activity activity)
  {
    if (!string.Equals(activity.Source.Name, InspectorCapture.ActivitySourceName, StringComparison.Ordinal))
    {
      return false;
    }

    string? stateTypeName = Tag(activity, InspectorCapture.StateTypeTagName);
    string? inspectorTag = Tag(activity, InspectorCapture.InspectorTagName);
    return InspectorCapture.ShouldCapture(stateTypeName, inspectorTag);
  }

  internal ExportResult ExportActivities(IReadOnlyList<Activity> activities)
  {
    List<SpanIngestItem> spans = [];
    foreach (Activity activity in activities)
    {
      SpanIngestItem? item = BuildItem(activity);
      if (item is not null)
      {
        spans.Add(item);
      }
    }

    if (spans.Count == 0)
    {
      return ExportResult.Success;
    }

    SpanIngestDocument document = new()
    {
      Spans = spans
    };

    try
    {
      string json = JsonSerializer.Serialize(document, SpanIngestJsonContext.Default.SpanIngestDocument);
      using HttpRequestMessage request = new(HttpMethod.Post, IngestUri)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };
      using HttpResponseMessage response = HttpClient.Send(request, CancellationToken.None);
      return response.IsSuccessStatusCode ? ExportResult.Success : ExportResult.Failure;
    }
    catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
    {
      return ExportResult.Failure;
    }
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      HttpClient.Dispose();
    }

    base.Dispose(disposing);
  }

  private static string? Tag(Activity activity, string name)
  {
    object? value = activity.GetTagItem(name);
    return value as string ?? value?.ToString();
  }

  private static Dictionary<string, string> StringTags(IEnumerable<KeyValuePair<string, object?>> tags)
  {
    Dictionary<string, string> attributes = [];
    foreach (KeyValuePair<string, object?> tag in tags)
    {
      if (tag.Value is null)
      {
        continue;
      }

      attributes[tag.Key] = tag.Value as string ?? tag.Value.ToString() ?? "";
    }

    return attributes;
  }
}
