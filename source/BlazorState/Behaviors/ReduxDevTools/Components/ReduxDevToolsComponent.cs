namespace BlazorState.Behaviors.ReduxDevTools
{
  using System;
  using Microsoft.AspNetCore.Blazor.Browser.Interop;
  using Microsoft.AspNetCore.Blazor.Components;

  /// <summary>
  ///
  /// </summary>
  public class ReduxDevToolsComponent : BlazorComponent
  {
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }

    protected override void OnInit()
    {
      ReduxDevToolsInterop.IsEnabled = RegisteredFunction.Invoke<bool>("blazor-state.ReduxDevTools.create");
      // We could send in the Store.GetSerializeState but it will be empty
      if (ReduxDevToolsInterop.IsEnabled)
        ReduxDevToolsInterop.DispatchInit("");
    }
  }
}