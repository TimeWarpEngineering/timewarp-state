namespace BlazorState.Behaviors.ReduxDevTools
{
  using BlazorState.Behaviors.JavaScriptInterop;
  using Microsoft.AspNetCore.Blazor.Components;

  /// <summary>
  /// Just sets the Instance for JsonRequestHandler
  /// </summary>
  public class JavaScriptInteropComponent : BlazorComponent
  {
    [Inject] private JsonRequestHandler JsonRequestHandler { get; set; }

    protected override void OnInit()
    {
      JavaScriptInstanceHelper.JsonRequestHandler = JsonRequestHandler;
    }
  }
}