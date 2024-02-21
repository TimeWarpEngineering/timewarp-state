#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Purple;

public partial class PurpleState
{
    // ReSharper disable once UnusedType.Global
    public static class Load
    {
        // ReSharper disable once UnusedType.Global
        public class Action : IAction { }

        // ReSharper disable once UnusedType.Global
        public class Handler
        (
          IStore store,
          IPersistenceService PersistenceService
        ): ActionHandler<Action>(store)
        {
            public override async System.Threading.Tasks.Task Handle(Action aAction, System.Threading.CancellationToken aCancellationToken)
            {
                try
                {
                    object? state = await PersistenceService.LoadState(typeof(PurpleState), PersistentStateMethod.LocalStorage);
                    if (state is PurpleState purpleState) Store.SetState(purpleState);
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
