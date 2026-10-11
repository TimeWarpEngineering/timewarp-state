#region Purpose
// Source-generated JSON type info for DevTools snapshots of CounterState.
#endregion

#region Design
// TimeWarp.State.Telemetry serializes snapshots only through caller JsonTypeInfo. This context is
// used when TIMEWARP_STATE_DEVTOOLS_ENDPOINT is set. It is not used for the metadata-only path.
#endregion

namespace Sample04Server;

/// <summary>
/// JSON contract for the telemetry sample's counter state.
/// </summary>
[JsonSerializable(typeof(CounterState))]
public sealed partial class SampleJsonContext : JsonSerializerContext;
