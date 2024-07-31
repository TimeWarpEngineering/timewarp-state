namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class PushRouteInfoActionSet
  {
    public class Action : IAction;

    internal class Handler : ActionHandler<Action>
    {
      private readonly NavigationManager NavigationManager;
      private readonly IJSRuntime JsRuntime;
      public Handler(NavigationManager navigationManager, IJSRuntime jsRuntime, IStore store) : base(store)
      {
        NavigationManager = navigationManager;
        JsRuntime = jsRuntime;
      }
      private RouteState RouteState => Store.GetState<RouteState>();

      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        SemaphoreSlim semaphoreSlim = Store.GetSemaphore(typeof(RouteState));
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
          string currentUri = NavigationManager.Uri;

          string title = await JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title");
          if (RouteState.RouteStack.TryPeek(out RouteInfo? routeInfo) && routeInfo.Url == currentUri)
          {
            // Update Title
            RouteState.RouteStack.Pop();
            var newRouteInfo = new RouteInfo(currentUri, title);
            RouteState.RouteStack.Push(newRouteInfo);
            return;
          }
          
          RouteState.RouteStack.Push(new RouteInfo(currentUri, title));
        }
        finally
        {
          semaphoreSlim.Release();
        }
      }
    }
  }
  public async Task PushRouteInfo(CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;

    await Sender.Send
    (
      new PushRouteInfoActionSet.Action(),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
