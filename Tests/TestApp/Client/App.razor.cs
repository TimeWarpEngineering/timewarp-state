namespace TestApp.Client;

using BlazorState.Features.JavaScriptInterop;
using BlazorState.Features.Routing;
using BlazorState.Pipeline.ReduxDevTools;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public partial class App : ComponentBase
{
  [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
#if ReduxDevToolsEnabled
  [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }
#endif

  [Inject] private TimeWarpNavigationManager RouteManager { get; set; }

  protected override async Task OnAfterRenderAsync(bool aFirstRender)
  {
#if ReduxDevToolsEnabled
    await ReduxDevToolsInterop.InitAsync();
#endif
    await JsonRequestHandler.InitAsync();
  }
}
