#region Purpose
// WebAssembly counter state for the telemetry sample. Distinct from the server CounterState.
#endregion

#region Design
// Sealed partial State<WasmCounterState>. Count has a private setter so only the action handler
// can change it. A separate type keeps the inspector from merging this store with the server counter.
#endregion

namespace Sample04Wasm.Features.Counter;

public sealed partial class WasmCounterState : State<WasmCounterState>
{
  public int Count { get; private set; }

  public override void Initialize()
  {
    Count = 3;
  }
}
