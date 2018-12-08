namespace ServerSideSample.App
{
  using System;
  using System.Threading.Tasks;
  using Blazor.Extensions.Storage;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Blazor.Components;

  public class AppModel : BlazorComponent
  {
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }
    [Inject] private RouteManager RouteManager { get; set; }
    [Inject] protected LocalStorage LocalStorage {get; set; }

    protected override async Task OnInitAsync()
    {
      await LocalStorage.SetItem("clientApplication", GetType().FullName);
      await ReduxDevToolsInterop.InitAsync();
    }
  }
}