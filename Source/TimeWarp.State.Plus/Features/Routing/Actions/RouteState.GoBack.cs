namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class GoBackActionSet
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
    
    internal class Handler : ActionHandler<Action>
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
        if (RouteState.IsRouteStackEmpty) return Task.CompletedTask;
        
        // Pop until we reach the one we want or the stack is empty
        RouteInfo target = null!;
        for (int i = 0; i <= action.Amount; i++) 
        {
          target = RouteState.RouteStack.Pop();
          if (RouteState.IsRouteStackEmpty) break;
        }

        NavigationManager.NavigateTo(target.Url);
        return Task.CompletedTask;
      }
    }
  }
  
  public async Task GoBack(CancellationToken cancellationToken, int amount = 1) => 
    await Sender.Send(new GoBackActionSet.Action(amount), cancellationToken);
}
