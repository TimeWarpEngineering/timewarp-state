namespace Middleware.Client.Features.Counter;

using BlazorState;

public partial class CounterState
{
    public class IncrementCountAction : IAction
    {
        public int Amount {get; set;}
    }
}
