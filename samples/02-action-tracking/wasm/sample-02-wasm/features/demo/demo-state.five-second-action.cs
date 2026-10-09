#region Purpose
// A tracked action that takes 5 seconds, used to show ActiveActionBehavior and the active-action display.
#endregion

#region Design
// The Action is marked [TrackAction]. The handler only awaits Task.Delay(5s) with the cancellation
// token and changes no state.
#endregion

namespace Sample02Wasm.Features.Demo;

partial class DemoState
{
    public static class FiveSecondActionSet
    {
        [TrackAction]
        public sealed class Action : IAction { }
        
        public sealed class Handler : StateActionHandler<Action>
        {
            public Handler(IStore store) : base(store) { }

            public override async ValueTask Handle
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
