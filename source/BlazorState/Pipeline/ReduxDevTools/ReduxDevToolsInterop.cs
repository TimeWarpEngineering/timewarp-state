namespace BlazorState.Pipeline.ReduxDevTools
{
  using System;
  using System.Threading.Tasks;
  using BlazorState.Services;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  public class ReduxDevToolsInterop
  {
    private const string JsFunctionName = "reduxDevTools.ReduxDevToolsDispatch";

    public ReduxDevToolsInterop(
      ILogger<ReduxDevToolsInterop> aLogger,
      IReduxDevToolsStore aStore,
      IJSRuntime aJSRuntime)
    {
      Logger = aLogger;
      Store = aStore;
      JSRuntime = aJSRuntime;
    }

    public bool IsEnabled { get; set; }
    private IJSRuntime JSRuntime { get; }
    private ILogger Logger { get; }
    private IReduxDevToolsStore Store { get; }

    public void Dispatch<TRequest>(TRequest aRequest, object aState)
    {
      if (IsEnabled)
      {
        Logger.LogDebug($"{GetType().Name}: {nameof(this.Dispatch)}");
        Logger.LogDebug($"{GetType().Name}: aRequest.GetType().FullName:{aRequest.GetType().FullName}");
        var reduxAction = new ReduxAction(aRequest);
        JSRuntime.InvokeAsync<object>(JsFunctionName, reduxAction, aState);
      }
    }

    public void DispatchInit(object aState)
    {
      if (IsEnabled)
        JSRuntime.InvokeAsync<object>(JsFunctionName, "init", aState);
    }

    public async Task InitAsync()
    {
      Logger.LogDebug("Init ReduxDevToolsInterop");
      const string ReduxDevToolsFactoryName = "ReduxDevToolsFactory";
      IsEnabled = await JSRuntime.InvokeAsync<bool>(ReduxDevToolsFactoryName);

      if (IsEnabled)
        DispatchInit(Store.GetSerializableState());
    }
  }
}