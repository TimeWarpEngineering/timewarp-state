namespace SampleDotNet8.Client.Features.Counter;

using BlazorState;
using BlazorState.Features.Persistence.Attributes;

[PersistentState(PersistentStateMethod.SessionStorage)]
public partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }
  public override void Initialize() => Count = 3;
}
