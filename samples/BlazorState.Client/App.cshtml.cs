namespace BlazorState.Client
{
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using Microsoft.AspNetCore.Blazor;
  using Microsoft.AspNetCore.Blazor.Components;

  public class AppModel:BlazorComponent
  {
    [Inject] private RouteManager RouteManager { get; set; }

    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }

    public RenderFragment SingletonComponents { get; set; }

    protected override void OnInit()
    {
      base.OnInit();
      SingletonComponents = builder =>
      {
        builder.OpenComponent<ReduxDevToolsComponent>(0);
        builder.CloseComponent();
      };
    }
  }
}
