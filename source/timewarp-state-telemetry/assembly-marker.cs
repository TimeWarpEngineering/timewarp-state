#region Purpose
// Marker type for this assembly plus the compile-time ClientPipeline behavior weave.
#endregion

#region Design
// MediatorAssembly + MediatorScope join the host's generated mediator graph the same way
// StateTransactionBehavior is woven: closed TelemetryBehavior<TAction,TResponse> types stay
// visible to the trimmer. Order 350 sits inside StateTransactionBehavior (300) and outside
// RenderSubscriptionsPostProcessor (400) so handler exceptions hit this catch before the
// transaction swallows them. Duration covers handler + render, not clone/Redux JS.
// Snapshots stay off until AddTimeWarpStateTelemetry sets IncludeSnapshots.
#endregion

[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(TelemetryBehavior<,>), order: 350, Scope = typeof(ClientPipeline))]

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Marker type for the TimeWarp.State.Telemetry assembly.
/// </summary>
public sealed class AssemblyMarker;
