#region Purpose
// Optional per-scope render kind a host records so telemetry can tag spans.
#endregion

#region Design
// The store Guid already distinguishes two pipelines. This name is only the host's label
// (prerender or interactive) when the host can tell. Telemetry reads Name at emit time.
// Observe is first-write-wins so one scope keeps the kind it was created under.
// A host that cannot tell leaves the service unregistered; spans then carry the store id alone.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Names for <see cref="IStateRenderKind.Name"/>.
/// </summary>
public static class StateRenderKinds
{
  /// <summary>Static server render. The store is thrown away when the response completes.</summary>
  public const string Prerender = "prerender";

  /// <summary>Interactive circuit or interactive WebAssembly session.</summary>
  public const string Interactive = "interactive";
}

/// <summary>
/// Per-scope render kind supplied by a host that can tell prerender from interactive.
/// </summary>
public interface IStateRenderKind
{
  /// <summary>
  /// <see cref="StateRenderKinds.Prerender"/>, <see cref="StateRenderKinds.Interactive"/>,
  /// or null before <see cref="Observe"/> runs.
  /// </summary>
  string? Name { get; }

  /// <summary>
  /// Records the render kind. The first call wins.
  /// </summary>
  /// <param name="isInteractive">True when the renderer is interactive.</param>
  void Observe(bool isInteractive);
}
