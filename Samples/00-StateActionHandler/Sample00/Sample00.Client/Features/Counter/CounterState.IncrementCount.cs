namespace Sample00.Client.Features.Counter;

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
        
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }
            
            private CounterState CounterState => Store.GetState<CounterState>();
            public override Task Handle(Action action, CancellationToken cancellationToken)
            {
                CounterState.Count += action.Amount;
                return Task.CompletedTask;
            }
        }
    }
}
