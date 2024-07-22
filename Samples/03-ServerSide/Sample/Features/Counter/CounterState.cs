// CounterState.cs
namespace Sample.Features.Counter;

using MediatR;
using TimeWarp.State;

internal sealed partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }
  public override void Initialize()
  {
    Count = 3;
  }
}
