namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class ChangeRouteActionSet
  {
    public sealed class Action : IAction
    {
      public Action(string newRoute) 
      {
        NewRoute = newRoute;
      }
      public string NewRoute { get; }
    }

    public sealed class Handler : ActionHandler<Action>
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

      public override ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
      {
        Logger.LogDebug("ChangeRouteAction.Handle NewRoute:{NewRoute}", action.NewRoute);
        string newAbsoluteUri = NavigationManager.ToAbsoluteUri(action.NewRoute).ToString();
        string absoluteUri = NavigationManager.Uri;

        if (absoluteUri != newAbsoluteUri)
        {
          NavigationManager.NavigateTo(newAbsoluteUri);
        }
        return new ValueTask<Unit>(Unit.Value);
      }
    }
  }
}
