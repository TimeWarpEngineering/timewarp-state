namespace TestApp.Client
{
  using System.Threading.Tasks;
  using BlazorState.Pipeline.ReduxDevTools;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Components;

  public class AppModel : ComponentBase
  {
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }

    // Injected so it iscreated by the container even though ide says not used.
    [Inject] private RouteManager RouteManager { get; set; }
    [Inject] IComponentContext ComponentContext { get; set; }

    //protected override async Task OnInitAsync()
    //{
    //  if (ComponentContext.IsConnected)
    //  {
    //    await JsonRequestHandler.InitAsync();
    //    await ReduxDevToolsInterop.InitAsync();
    //  }
    //}

    protected override async Task OnAfterRenderAsync()
    {
      if (ComponentContext.IsConnected)
      {
        await JsonRequestHandler.InitAsync();
        await ReduxDevToolsInterop.InitAsync();
      }
    }

  }
}