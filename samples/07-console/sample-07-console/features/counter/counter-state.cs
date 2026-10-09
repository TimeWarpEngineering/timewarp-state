#region Purpose
// Counter state for the console sample: a single Count value.
#endregion

#region Design
// Sealed partial State<CounterState>. Count has a private setter so only the nested handler can change it.
// Initialize sets Count to 3.
#endregion

namespace Sample07Console.Features.Counter;

public sealed partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }

  public override void Initialize() => Count = 3;
}
