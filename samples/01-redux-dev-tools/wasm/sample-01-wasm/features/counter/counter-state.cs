#region Purpose
// Counter state for the Redux DevTools sample: a single Count value.
#endregion

#region Design
// Sealed partial State<CounterState>. Count has a private setter so only the action handlers in the
// partial files can change it. Initialize sets Count to 3.
#endregion

namespace Sample01Wasm.Features.Counter;

public sealed partial class CounterState : State<CounterState>
{
    public int Count { get; private set; }
    
    public override void Initialize()
    {
        Count = 3;
    }
}
