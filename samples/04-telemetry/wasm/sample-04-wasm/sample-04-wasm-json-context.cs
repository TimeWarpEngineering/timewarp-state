#region Purpose
// Source-generated JSON type info for DevTools snapshots of WasmCounterState.
#endregion

#region Design
// TimeWarp.State.Telemetry serializes snapshots only through caller JsonTypeInfo. This context is
// used when the inspector origin resolves. It is not used for the metadata-only path.
#endregion

namespace Sample04Wasm;

/// <summary>
/// JSON contract for the WebAssembly counter state.
/// </summary>
[JsonSerializable(typeof(WasmCounterState))]
public sealed partial class Sample04WasmJsonContext : JsonSerializerContext;
