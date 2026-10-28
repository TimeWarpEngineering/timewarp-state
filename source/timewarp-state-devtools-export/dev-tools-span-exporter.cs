#region Purpose
// Posts completed TimeWarp.State activities to the DevTools ingest endpoint.
#endregion

#region Design
// BaseExporter.Export is synchronous; the batch processor calls it off the request thread.
// HttpClient.Send stays on that thread. Failures return Failure and do not throw, so a stopped
// inspector cannot fail the inspected app. Only ActivitySource TimeWarp.State is copied.
// Framework sources (Microsoft.AspNetCore, Microsoft.AspNetCore.Components,
// Microsoft.AspNetCore.Components.Server.Circuits, Microsoft.AspNetCore.SignalR.Server) are not
// copied, so a host that subscribes to them for the Aspire dashboard does not fill the inspector.
// InspectorCapture drops the inspector's own spans before the POST. This path is the server
// exporter. The browser uses TimeWarp.State.DevTools.Wasm because this Send cannot run there.
// A copied span with no timewarp.state.runtime tag is stamped server.
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

    SpanIngestItem? item = SpanIngestMapping.FromActivity(activity);
    if (item is null)
    {
      return null;
    }

    if (!item.Attributes.ContainsKey(InspectorCapture.RuntimeTagName))
    {
      item.Attributes[InspectorCapture.RuntimeTagName] = InspectorCapture.ServerRuntime;
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
    catch (Exception exception) when (exception is not OutOfMemoryException)
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
}
