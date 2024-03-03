namespace SampleDotNet8.Client.Features.Counter;

[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }

  public CounterState() { }

  [JsonConstructor]
  public CounterState(Guid guid, int count)
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
