#region Purpose
// IncrementCount action for the Redux DevTools sample: adds Amount to Count.
#endregion

#region Design
// Nested ActionSet (Action + Handler) inside the partial CounterState so the handler can set the private
// Count. The handler is synchronous and returns a completed ValueTask.
#endregion

namespace Sample01Wasm.Features.Counter;

partial class CounterState
{
    public static class IncrementCountActionSet
    {
        public sealed class Action : IAction
        {
            public int Amount { get; }
            
            public Action(int amount)
            {
                Amount = amount;
            }
        }
        
        public sealed class Handler : StateActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }
            
            private CounterState CounterState => Store.GetState<CounterState>();

            public override ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
                CounterState.Count += action.Amount;
                return default;
            }
        }
    }
}
