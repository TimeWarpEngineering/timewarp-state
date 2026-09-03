namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class GoBackActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; }

      public Action(int amount = 1)
      {
        Amount = amount;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
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

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if (RouteState.IsRouteStackEmpty || action.Amount == 0) return default;

        // Determine how far back we can actually go. The current page occupies the top of the stack,
        // so the destination must remain — clamp to Count - 1 (matches CanGoBack => Count > 1).
        int amountToGoBack = Math.Min(action.Amount, RouteState.RouteStack.Count - 1);
        if (amountToGoBack <= 0) return default;

        // Pop off the routes we don't need
        for (int i = 0; i < amountToGoBack; i++)
        {
          RouteState.RouteStack.Pop();
        }

        var target = RouteState.RouteStack.Peek();
        NavigationManager.NavigateTo(target.Url);
        return default;
      }
    }
  }
}
