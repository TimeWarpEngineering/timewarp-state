namespace Test.App.Client.Features.Counter2;

using BlazorState.Features.Persistence.Attributes;
using System.Text.Json.Serialization;

[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class BlueState : State<BlueState>
{
  public int Count { get; private set; }

  public BlueState() { }

  [JsonConstructor]
  public BlueState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  public override void Initialize()
  {
    Console.WriteLine("Counter2State.Initialize");
    Count = 2;
  }
}
