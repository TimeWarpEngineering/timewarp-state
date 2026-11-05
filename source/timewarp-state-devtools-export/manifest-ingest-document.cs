#region Purpose
// JSON contract posted to the DevTools manifest ingest endpoint.
#endregion

#region Design
// Each entry is one assembly's manifest document kept as the raw JSON text the generator wrote, so the
// receiver parses it with StateManifest.Parse and the exporter needs no manifest model of its own.
// Source-generated JsonTypeInfo keeps the serializer off reflection. Property names follow the span document.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Body of <c>POST /ingest/manifest</c>.
/// </summary>
public sealed class ManifestIngestDocument
{
  /// <summary>Raw State manifest JSON documents, one per assembly.</summary>
  public List<string> Manifests { get; set; } = [];
}

/// <summary>
/// Source-generated JSON contract for <see cref="ManifestIngestDocument"/>.
/// </summary>
[JsonSerializable(typeof(ManifestIngestDocument))]
public sealed partial class ManifestIngestJsonContext : JsonSerializerContext;
