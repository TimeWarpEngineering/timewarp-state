namespace Sample.Client;

using System.Threading.Tasks;
using TimeWarp.Features.ReduxDevTools;
using TimeWarp.Features.JavaScriptInterop;
using TimeWarp.Features.Routing;
using Microsoft.AspNetCore.Components;

public partial class App : ComponentBase
{
  [Inject] private JsonRequestHandler JsonRequestHandler { get; set; } = null!;
  [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; } = null!;

  [Inject]
  [System.Diagnostics.CodeAnalysis.SuppressMessage
    (
      "CodeQuality", 
      "IDE0051:Remove unused private members", 
      Justification = "It is used, the constructor has side effects "
    )
  ]
  private TimeWarpNavigationManager TimeWarpNavigationManager { get; set; } = null!;

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    // TODO:
    throw new Exception("do I hit?"); 
    await ReduxDevToolsInterop.InitAsync();
    await JsonRequestHandler.InitAsync();
  }
}
