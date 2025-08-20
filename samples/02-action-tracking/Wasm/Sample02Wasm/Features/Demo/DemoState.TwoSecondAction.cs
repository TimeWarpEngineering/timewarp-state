namespace Sample02Wasm.Features.Demo;

partial class DemoState
{
    public static class TwoSecondActionSet
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
                // Simulate a 2-second action
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}
