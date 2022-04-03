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

  [Inject] private RouteManager RouteManager { get; set; }

  protected override async Task OnAfterRenderAsync(bool aFirstRender)
  {
    ; // Keeps dotner format from thinking it is single line
#if ReduxDevToolsEnabled
    await ReduxDevToolsInterop.InitAsync();
#endif
    await JsonRequestHandler.InitAsync();
  }
}
