namespace TestApp.Client
{
  using System.Threading.Tasks;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Components;

  public class AppModel : ComponentBase
  {
    // Injected so they are created by the container even though ide says not used.
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }
    [Inject] private RouteManager RouteManager { get; set; }

    protected override async Task OnInitAsync()
    {
      await JsonRequestHandler.InitAsync();
      await ReduxDevToolsInterop.InitAsync();
    }
  }
}