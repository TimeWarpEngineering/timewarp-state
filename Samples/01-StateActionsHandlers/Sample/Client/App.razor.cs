namespace Sample.Client;

using BlazorState.Features.JavaScriptInterop;
using BlazorState.Features.Routing;
using BlazorState.Pipeline.ReduxDevTools;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public partial class App : ComponentBase
{
  [Inject] private JsonRequestHandler JsonRequestHandler { get; set; } = null!;
  [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; } = null!;

  // Injected so it is created by the container. Even though the IDE says it is not used, it is.
  [Inject] private RouteManager RouteManager { get; set; } = null!;

  protected override async Task OnAfterRenderAsync(bool aFirstRender)
  {
    await ReduxDevToolsInterop.InitAsync();
    await JsonRequestHandler.InitAsync();
  }
}
