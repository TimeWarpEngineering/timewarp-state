namespace Sample.Client.Features.Counter;

using MediatR;

public partial class CounterState : State<CounterState>
{
  public int Count { get; private set; }
  public CounterState(ISender sender) : base(sender) {}
  public override void Initialize() => Count = 3;
}
