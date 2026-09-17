#region Purpose
// Record the current URI on RouteState, truncating back to it when that URI is already in the stack.
#endregion

#region Design
// Same-URL-on-top updates the title in place (page re-render). An older matching URL pops everything
// above it so sidebar revisits and browser Back shrink the breadcrumb instead of duplicating.
// No stack-depth cap: truncate-on-revisit is the unbounded-duplicate fix; unique-URL growth is a real
// trail (TwBreadcrumb.MaxLinks already limits display). Dropping oldest would hide Home. Revisit if
// a consumer reports unique-URL blowup.
#endregion

namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class PushRouteInfoActionSet
  {
    public sealed class Action : IAction;

    public sealed class Handler : StateActionHandler<Action>
    {
      private readonly NavigationManager NavigationManager;
      private readonly IJSRuntime JsRuntime;
      public Handler(NavigationManager navigationManager, IJSRuntime jsRuntime, IStore store) : base(store)
      {
        NavigationManager = navigationManager;
        JsRuntime = jsRuntime;
      }
      private RouteState RouteState => Store.GetState<RouteState>();

      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        SemaphoreSlim? semaphoreSlim = Store.GetSemaphore(typeof(RouteState));
        if (semaphoreSlim == null) return;
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
          string currentUri = NavigationManager.Uri;

          string title = await JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title");
          TruncateToOrPush(currentUri, title);
        }
        finally
        {
          semaphoreSlim.Release();
        }
      }

      private void TruncateToOrPush(string url, string title)
      {
        RouteInfo[] routes = RouteState.RouteStack.ToArray();
        int matchIndex = Array.FindIndex(routes, routeInfo => routeInfo.Url == url);
        if (matchIndex >= 0)
        {
          for (int i = 0; i <= matchIndex; i++)
          {
            RouteState.RouteStack.Pop();
          }
        }

        RouteState.RouteStack.Push(new RouteInfo(url, title));
      }
    }
  }
}
