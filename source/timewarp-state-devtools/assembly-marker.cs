#region Purpose
// Joins the DevTools assembly to the ClientPipeline mediator graph.
#endregion

#region Design
// MediatorAssembly plus MediatorScope, same as a sample host. This assembly does not reference
// TimeWarp.State.Telemetry and does not weave TelemetryBehavior, so inspector actions are not
// activities on ActivitySource TimeWarp.State.
#endregion

[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(ClientPipeline))]

namespace TimeWarp.State.DevTools;

/// <summary>
/// Marker type for the TimeWarp.State.DevTools assembly.
/// </summary>
public sealed class AssemblyMarker;
