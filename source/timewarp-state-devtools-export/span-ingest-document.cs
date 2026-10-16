#region Purpose
// JSON contract posted to the DevTools ingest endpoint.
#endregion

#region Design
// A narrow document, not OTLP. The 1.19 exporter has no HTTP/JSON protocol, and a protobuf decoder
// is a dependency this dev-only channel does not need. Source-generated JsonTypeInfo keeps the
// serializer off reflection.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Body of <c>POST /ingest/spans</c>.
/// </summary>
#if DEVTOOLS_WASM
internal sealed class SpanIngestDocument
#else
public sealed class SpanIngestDocument
#endif
{
  /// <summary>Spans from one export batch.</summary>
  public List<SpanIngestItem> Spans { get; set; } = [];
}

/// <summary>
/// One completed TimeWarp.State activity.
/// </summary>
#if DEVTOOLS_WASM
internal sealed class SpanIngestItem
#else
public sealed class SpanIngestItem
#endif
{
  /// <summary>Activity name.</summary>
  public string Name { get; set; } = "";

  /// <summary>W3C trace id, or empty.</summary>
  public string TraceId { get; set; } = "";

  /// <summary>W3C span id, or empty.</summary>
  public string SpanId { get; set; } = "";

  /// <summary><see cref="ActivityStatusCode"/> name.</summary>
  public string Status { get; set; } = "";

  /// <summary>UTC start, unix milliseconds.</summary>
  public long StartTimeUnixMilliseconds { get; set; }

  /// <summary>Activity duration in milliseconds.</summary>
  public double DurationMilliseconds { get; set; }

  /// <summary>Stringified span tags.</summary>
  public Dictionary<string, string> Attributes { get; set; } = [];

  /// <summary>Activity events (snapshot and diff).</summary>
  public List<SpanIngestEvent> Events { get; set; } = [];
}

/// <summary>
/// One activity event.
/// </summary>
#if DEVTOOLS_WASM
internal sealed class SpanIngestEvent
#else
public sealed class SpanIngestEvent
#endif
{
  /// <summary>Event name (<c>state.snapshot</c> or <c>state.diff</c>).</summary>
  public string Name { get; set; } = "";

  /// <summary>Stringified event tags.</summary>
  public Dictionary<string, string> Attributes { get; set; } = [];
}

/// <summary>
/// Source-generated JSON contract for <see cref="SpanIngestDocument"/>.
/// </summary>
[JsonSerializable(typeof(SpanIngestDocument))]
#if DEVTOOLS_WASM
internal sealed partial class SpanIngestJsonContext : JsonSerializerContext;
#else
public sealed partial class SpanIngestJsonContext : JsonSerializerContext;
#endif
