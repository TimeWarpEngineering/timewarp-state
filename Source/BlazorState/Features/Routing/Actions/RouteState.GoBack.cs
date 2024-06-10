namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class GoBack
  {
    [UsedImplicitly]
    public class Action : IAction
    {
      public int Amount { get; }
      public Action(int amount = 1)
      {
        Amount = amount;
      }
    }
    
    internal class GoBackHandler : ActionHandler<Action>
    {
      private readonly NavigationManager NavigationManager;
      public GoBackHandler
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
        if (RouteState.IsHistoryEmpty) return Task.CompletedTask;

        RouteState.GoingBack = true;
        // Pop off the ones we don't want
        RouteInfo target = null;
        for (int i = 0; i < action.Amount; i++) 
        {
          target = RouteState.HistoryStack.Pop();
          if (RouteState.IsHistoryEmpty) break;
        }
        // Navigate to the one we do want.
        NavigationManager.NavigateTo(target!.Url);
        return Task.CompletedTask;
      }
    }
  }
}
