namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class ChangeRoute
  {
    public class Action : IAction
    {
      public string NewRoute { get; init; }
    }

    internal class ChangeRouteHandler : ActionHandler<Action>
    {
      private readonly ILogger Logger;
      private readonly NavigationManager NavigationManager;
      private readonly IJSRuntime JsRuntime;
      public ChangeRouteHandler
      (
        IStore store,
        NavigationManager navigationManager,
        ILogger<ChangeRouteHandler> logger,
        IJSRuntime jsRuntime
      ) : base(store)
      {
        NavigationManager = navigationManager;
        Logger = logger;
        JsRuntime = jsRuntime;
      }

      private RouteState RouteState => Store.GetState<RouteState>();

      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Logger.LogDebug("ChangeRouteAction.Handle-NewRoute:{NewRoute}", action.NewRoute);
        string newAbsoluteUri = NavigationManager.ToAbsoluteUri(action.NewRoute).ToString();
        string absoluteUri = NavigationManager.Uri;

        if (absoluteUri != newAbsoluteUri)
        {
          // TimeWarpNavigationManager OnLocationChanged will fire this ChangeRouteRequest again
          // and the second time we will hit the `else` clause.
          NavigationManager.NavigateTo(newAbsoluteUri);
        }
        else if (RouteState.Route != newAbsoluteUri)
        {
          if (!RouteState.GoingBack)
          {
            string title = await JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title");
            RouteState.HistoryStack.Push(new RouteInfo(RouteState.Route, title));
          }
          else
          {
            RouteState.GoingBack = false;
          }
          RouteState.Route = newAbsoluteUri;
        }
      }
    }
  }
}
