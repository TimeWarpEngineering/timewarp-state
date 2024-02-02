namespace Test.App.Client.Features.Counter;

using BlazorState.Features.Persistence.Attributes;
using System.Text.Json.Serialization;

[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class CounterOneState : State<CounterOneState>
{
  public int Count { get; private set; }

  public CounterOneState() { }

  [JsonConstructor]
  public CounterOneState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  public override void Initialize()
  {
    Console.WriteLine("CounterState.Initialize");
    Count = 1;
  }
}
