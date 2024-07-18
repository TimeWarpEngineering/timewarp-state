namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class ChangeRouteActionSet
  {
    public class Action : IAction
    {
      public Action(string newRoute) 
      {
        NewRoute = newRoute;
      }
      public string NewRoute { get; }
    }

    internal class Handler : ActionHandler<Action>
    {
      private readonly ILogger Logger;
      private readonly NavigationManager NavigationManager;
      public Handler
      (
        IStore store,
        NavigationManager navigationManager,
        ILogger<Handler> logger
      ) : base(store)
      {
        NavigationManager = navigationManager;
        Logger = logger;
      }

      public override Task Handle(Action action, CancellationToken cancellationToken)
      {
        Logger.LogDebug("ChangeRouteAction.Handle NewRoute:{NewRoute}", action.NewRoute);
        string newAbsoluteUri = NavigationManager.ToAbsoluteUri(action.NewRoute).ToString();
        string absoluteUri = NavigationManager.Uri;

        if (absoluteUri != newAbsoluteUri)
        {
          NavigationManager.NavigateTo(newAbsoluteUri);
        }
        return Task.CompletedTask;
      }
    }
  }
  public async Task ChangeRoute(string newRoute, CancellationToken cancellationToken) =>
    await Sender.Send(new RouteState.ChangeRouteActionSet.Action(newRoute), cancellationToken);
}
