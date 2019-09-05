namespace TestApp.Client
{
  using System.Threading.Tasks;
  using BlazorState.Pipeline.ReduxDevTools;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Components;

  public class AppBase : ComponentBase
  {
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }

    // Injected so it is created by the container. Even though the IDE says it is not used it is.
    [Inject] private RouteManager RouteManager { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      await ReduxDevToolsInterop.InitAsync();
      await JsonRequestHandler.InitAsync();
    }
  }
}