namespace TimeWarp.State.Telemetry.Tests;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TelemetryTestState))]
internal partial class TelemetryTestJsonContext : JsonSerializerContext;
