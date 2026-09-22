#region Purpose
// Marker type for this assembly plus the compile-time ClientPipeline behavior weave.
#endregion

#region Design
// MediatorAssembly + MediatorScope join the host's generated mediator graph the same way
// StateTransactionBehavior is woven: closed TelemetryBehavior<TAction,TResponse> types stay
// visible to the trimmer. Order 50 is outermost so duration covers the full State pipeline.
// Snapshots stay off until AddTimeWarpStateTelemetry sets IncludeSnapshots.
#endregion

[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(TelemetryBehavior<,>), order: 50, Scope = typeof(ClientPipeline))]

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Marker type for the TimeWarp.State.Telemetry assembly.
/// </summary>
public sealed class AssemblyMarker;
