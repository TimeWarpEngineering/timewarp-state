namespace TimeWarp.Features.Routing;

public partial class RouteState
{
  public static class PushRouteInfo
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
        if (RouteState.RouteStack.Peek().Url != NavigationManager.Uri)
        {
          string title = await JsRuntime.InvokeAsync<string>("eval", cancellationToken, "document.title");
          RouteState.RouteStack.Push(new RouteInfo(NavigationManager.Uri, title));
        }
      }
    }
  }
}
