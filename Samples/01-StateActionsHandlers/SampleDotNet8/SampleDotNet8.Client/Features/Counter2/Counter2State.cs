namespace SampleDotNet8.Client.Features.Counter2;

using BlazorState.Features.Persistence.Attributes;
using System.Text.Json.Serialization;

[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class Counter2State : State<Counter2State>
{
  public int Count { get; private set; }

  public Counter2State() { }

  [JsonConstructor]
  public Counter2State(Guid guid, int count)
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
