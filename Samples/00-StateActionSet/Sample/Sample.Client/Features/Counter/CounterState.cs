// CounterState.cs
namespace Sample.Client.Features.Counter;

using TimeWarp.State;

public partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }
  public override void Initialize() => Count = 3;
}
