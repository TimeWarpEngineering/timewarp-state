namespace Sample00Auto.Client.Features.Counter;

public sealed partial class CounterState : State<CounterState>
{
    public int Count { get; private set; }
    public override void Initialize()
    {
        Count = 3;
    }
}
