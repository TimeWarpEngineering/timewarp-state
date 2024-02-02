namespace Test.App.Client.Features.Counter;

using System.Text.Json.Serialization;

public partial class CounterZeroState : State<CounterZeroState>
{

  public int Count { get; private set; }

  public CounterZeroState() { }

  [JsonConstructor]
  public CounterZeroState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize() => Count = 3;
}
