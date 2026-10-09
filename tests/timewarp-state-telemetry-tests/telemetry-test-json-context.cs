#region Purpose
// Source-generated JSON context for TelemetryTestState.
#endregion

#region Design
// camelCase naming, matching how the snapshot tests expect state JSON.
#endregion

namespace TimeWarp.State.Telemetry.Tests;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TelemetryTestState))]
internal partial class TelemetryTestJsonContext : JsonSerializerContext;
