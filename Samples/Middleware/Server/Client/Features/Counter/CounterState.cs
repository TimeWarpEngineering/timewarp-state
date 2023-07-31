namespace Middleware.Client.Features.Counter;

using BlazorState;

public partial class CounterState : State<CounterState>
{
    public int Count {get; private set;}
    public override void Initialize() => Count = 3;
}

