namespace BlazorState.Client
{
  using System.Threading.Tasks;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Blazor.Components;

  public class AppModel : BlazorComponent
  {
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }
    [Inject] private RouteManager RouteManager { get; set; }

    protected override async Task OnInitAsync() => await ReduxDevToolsInterop.InitAsync();
  }
}