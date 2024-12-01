namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class GoBackActionSet
  {
    internal sealed class Action : IAction
    {
      public int Amount { get; }

      public Action(int amount = 1)
      {
        Amount = amount;
      }
    }

    internal sealed class Handler : ActionHandler<Action>
    {
      private readonly NavigationManager NavigationManager;

      public Handler
      (
        IStore store,
        NavigationManager navigationManager
      ) : base(store)
      {
        NavigationManager = navigationManager;
      }

      private RouteState RouteState => Store.GetState<RouteState>();

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        if (RouteState.IsRouteStackEmpty || action.Amount == 0) return Task.CompletedTask;

        // Determine how far back we can actually go
        int amountToGoBack = Math.Min(action.Amount, RouteState.RouteStack.Count);

        // Pop off the routes we don't need
        for (int i = 0; i < amountToGoBack; i++)
        {
          RouteState.RouteStack.Pop();
        }

        var target = RouteState.RouteStack.Peek();
        NavigationManager.NavigateTo(target.Url);
        return Task.CompletedTask;
      }
    }
  }
}
