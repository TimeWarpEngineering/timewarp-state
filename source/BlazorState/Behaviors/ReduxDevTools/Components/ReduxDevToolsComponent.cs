namespace BlazorState.Behaviors.ReduxDevTools
{
  using Microsoft.AspNetCore.Blazor.Browser.Interop;
  using Microsoft.AspNetCore.Blazor.Components;

  /// <summary>
  ///
  /// </summary>
  public class ReduxDevToolsComponent : BlazorComponent
  {
    public bool IsEnabled { get; set; }
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }

    protected override void OnInit()
    {
      IsEnabled = RegisteredFunction.Invoke<bool>("blazor-state.ReduxDevTools.create");
      // We could send in the Store.GetSerializeState but it will be empty
      if (IsEnabled) ReduxDevToolsInterop.DispatchInit("");
    }
  }
}