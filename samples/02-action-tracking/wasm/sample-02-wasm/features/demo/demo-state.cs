#region Purpose
// Empty state that owns the demo actions for the action-tracking sample.
#endregion

#region Design
// Holds no data. It exists so the timed actions have a State to belong to; Initialize does nothing.
#endregion

namespace Sample02Wasm.Features.Demo;

public sealed partial class DemoState : State<DemoState>
{
    public override void Initialize() { }
}
