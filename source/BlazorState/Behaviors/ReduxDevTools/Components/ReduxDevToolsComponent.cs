namespace BlazorState.Behaviors.ReduxDevTools
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Blazor.Components;
  using Microsoft.JSInterop;

  /// <summary>
  /// Add this component to Client App to use ReduxDevTools
  /// </summary>
  /// <example>
  /// TODO:
  /// </example>
  public class ReduxDevToolsComponent : BlazorComponent
  {
    [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; }


    protected async override Task OnInitAsync()
    {
      const string ReduxDevToolsFactoryName = "reduxDevToolsFactory";
      ReduxDevToolsInterop.IsEnabled = await JSRuntime.Current.InvokeAsync<bool>(ReduxDevToolsFactoryName);
      // We could send in the Store.GetSerializeState but it will be empty
      if (ReduxDevToolsInterop.IsEnabled)
        ReduxDevToolsInterop.DispatchInit(string.Empty);
    }
  }
}