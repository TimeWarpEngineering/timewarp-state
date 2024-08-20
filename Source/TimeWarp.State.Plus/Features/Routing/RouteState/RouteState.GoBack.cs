﻿namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class GoBackActionSet
  {
    [UsedImplicitly]
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
  
  public async Task GoBack(int amount = 1, CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;

    await Sender.Send
    (
      new GoBackActionSet.Action(amount),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
