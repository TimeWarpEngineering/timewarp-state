#region Purpose
// Action that resets the whole Store and routes back to "/", used by the Reset Store page and its E2E test.
#endregion

#region Design
// The handler is a plain IRequestHandler rather than StateActionHandler because it acts on the IStore itself, not on
// one state. It calls Store.Reset() and then RouteState.ChangeRoute("/").
#endregion

namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  public static class ResetStoreActionSet
  {
    public sealed class Action : IAction;

    internal sealed class Handler : IRequestHandler<Action>
    {
      private readonly IStore Store;
      public Handler(IStore store)
      {
        Store = store;
      }
      public async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Store.Reset();
        await Store.GetState<RouteState>().ChangeRoute(newRoute: "/", cancellationToken);
      }
    }
  }
}
