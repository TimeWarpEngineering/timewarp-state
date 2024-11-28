namespace Sample02Wasm.Features.Demo;

partial class DemoState
{
    public static class LongActionSet
    {
        [TrackAction]
        public sealed class Action : IAction { }
        
        public sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            public override async Task Handle
            (
                Action action,
                CancellationToken cancellationToken
            )
            {
                // Simulate a 5-second action
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
