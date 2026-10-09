#region Purpose
// CounterState: a single Count, the state most integration and E2E tests use.
#endregion

#region Design
// The parameterless constructor serves DI and cloning; the [JsonConstructor] (guid, count) lets it be deserialized
// with its Guid. Initialize sets Count to 3.
#endregion

namespace Test.App.Client.Features.Counter;

public sealed partial class CounterState : State<CounterState>
{

  public int Count { get; private set; }

  public CounterState() { }

  [JsonConstructor]
  public CounterState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize() => Count = 3;
}
