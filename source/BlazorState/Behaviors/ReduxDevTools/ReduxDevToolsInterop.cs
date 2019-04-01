namespace BlazorState.Behaviors.ReduxDevTools
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
      BlazorHostingLocation aBlazorHostingLocation)
    {
      Logger = aLogger;
      Store = aStore;
      BlazorHostingLocation = aBlazorHostingLocation;
    }

    public bool IsEnabled { get; set; }
    private BlazorHostingLocation BlazorHostingLocation { get; }
    private ILogger Logger { get; }
    private IReduxDevToolsStore Store { get; }

    public void Dispatch<TRequest>(TRequest aRequest, object aState)
    {
      if (IsEnabled)
      {
        Logger.LogDebug($"{GetType().Name}: {nameof(this.Dispatch)}");
        Logger.LogDebug($"{GetType().Name}: aRequest.GetType().FullName:{aRequest.GetType().FullName}");
        var reduxAction = new ReduxAction(aRequest);
        JSRuntime.Current.InvokeAsync<object>(JsFunctionName, reduxAction, aState);
      }
    }

    public void DispatchInit(object aState)
    {
      if (IsEnabled)
        JSRuntime.Current.InvokeAsync<object>(JsFunctionName, "init", aState);
    }

    public async Task InitAsync()
    {
      Console.WriteLine("Init ReduxDevToolsInterop");
      if (BlazorHostingLocation.IsClientSide) // Only init if running in WASM
      {
        Console.WriteLine("Running in WASM");
        const string ReduxDevToolsFactoryName = "ReduxDevToolsFactory";
        IsEnabled = await JSRuntime.Current.InvokeAsync<bool>(ReduxDevToolsFactoryName);

        if (IsEnabled)
          DispatchInit(Store.GetSerializableState());
      }
    }
  }
}