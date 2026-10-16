#region Purpose
// Copies one TimeWarp.State activity into the DevTools ingest document.
#endregion

#region Design
// Shared by the server exporter and the WASM exporter (the WASM project compiles this file).
// The copy is complete before the method returns, because ActivityStopped may dispose the activity
// as soon as the callback returns. No HTTP and no runtime stamp live here: each exporter stamps
// timewarp.state.runtime for the process it runs in.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Maps a completed <see cref="Activity"/> to <see cref="SpanIngestItem"/>.
/// </summary>
internal static class SpanIngestMapping
{
  /// <summary>
  /// Builds an ingest item, or null when <paramref name="activity"/> is not a captured TimeWarp.State span.
  /// </summary>
  internal static SpanIngestItem? FromActivity(Activity activity)
  {
    ArgumentNullException.ThrowIfNull(activity);
    if (!string.Equals(activity.Source.Name, InspectorCapture.ActivitySourceName, StringComparison.Ordinal))
    {
      return null;
    }

    string? stateTypeName = Tag(activity, InspectorCapture.StateTypeTagName);
    string? inspectorTag = Tag(activity, InspectorCapture.InspectorTagName);
    if (!InspectorCapture.ShouldCapture(stateTypeName, inspectorTag))
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
