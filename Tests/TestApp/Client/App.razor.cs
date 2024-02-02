namespace TestApp.Client;

using BlazorState.Features.JavaScriptInterop;
using BlazorState.Features.Routing;
using BlazorState.Pipeline.ReduxDevTools;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public partial class App : ComponentBase
{
  [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
  [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }

  [Inject] private TimeWarpNavigationManager TimeWarpNavigationManager { get; set; }

  protected override async Task OnAfterRenderAsync(bool aFirstRender)
  {
    await ReduxDevToolsInterop.InitAsync();
    await JsonRequestHandler.InitAsync();
  }
}
