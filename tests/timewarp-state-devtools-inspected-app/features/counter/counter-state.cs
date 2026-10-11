#region Purpose
// Counter state for the inspected-app fixture: a single Count value.
#endregion

#region Design
// Sealed partial State of CounterState. Count has a private setter so only the action handlers in the partial files can change it. The clone generator in this compilation registers CounterState and nothing from the DevTools app.
#endregion

namespace InspectedApp.Features.Counter;

/// <summary>
/// Count held by the inspected-app fixture.
/// </summary>
public sealed partial class CounterState : State<CounterState>
{
  /// <summary>
  /// Current count.
  /// </summary>
  public int Count { get; private set; }

  /// <inheritdoc />
  public override void Initialize()
  {
    Count = 3;
  }
}
