#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Blue;

using TimeWarp.Features.Persistence;
using TimeWarp.State;

public partial class BlueState
{
    public static class Load
    {
        public class Action : IAction { }

        public class Handler
        (
          IStore store,
          IPersistenceService PersistenceService
        ): ActionHandler<Action>(store)
        {
            public override async System.Threading.Tasks.Task Handle(Action action, System.Threading.CancellationToken cancellationToken)
            {
                try
                {
                    object? state = await PersistenceService.LoadState(typeof(BlueState), PersistentStateMethod.SessionStorage);
                    if (state is BlueState blueState) Store.SetState(blueState);
                }
                catch (Exception)
                {
                    // if this is a JavaScript not available exception then we are prerendering and just swallow it
                }
            }
        }
    }
}
#pragma warning restore CS1591
