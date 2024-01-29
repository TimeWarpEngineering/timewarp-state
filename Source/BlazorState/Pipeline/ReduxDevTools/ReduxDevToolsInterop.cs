namespace BlazorState.Pipeline.ReduxDevTools;

public class ReduxDevToolsInterop
{
  private const string JsFunctionName = "reduxDevTools.ReduxDevToolsDispatch";

  private readonly IJSRuntime JSRuntime;

  private readonly ILogger Logger;

  private readonly IReduxDevToolsStore Store;

  private readonly ReduxDevToolsOptions ReduxDevToolsOptions;
  private bool IsInitialized = false; 

  public bool IsEnabled { get; set; }


  public ReduxDevToolsInterop
  (
    ILogger<ReduxDevToolsInterop> aLogger,
    IReduxDevToolsStore aStore,
    ReduxDevToolsOptions aReduxDevToolsOptions,
    IJSRuntime aJSRuntime
  )
  {
    Logger = aLogger;
    Store = aStore;
    ReduxDevToolsOptions = aReduxDevToolsOptions;
    JSRuntime = aJSRuntime;
  }

  public async Task DispatchAsync<TRequest>(TRequest aRequest, object aState, string aStackTrace)
  {
    if (IsEnabled)
    {
      var reduxAction = new ReduxAction(aRequest);

      Logger.LogDebug
      (
        EventIds.ReduxDevToolsInterop_DispatchingRequest,
        "dispatching aRequest.GetType().FullName:{TypeFullName} to ReduxDevTools",
        aRequest.GetType().FullName
      );

      await JSRuntime.InvokeAsync<object>(JsFunctionName, reduxAction, aState, aStackTrace);
    }
  }

  public async Task DispatchInitAsync(object aState)
  {
    if (IsEnabled)
    {
      Logger.LogDebug(EventIds.ReduxDevToolsInterop_DispatchingInit, "dispatching init to ReduxDevTools");
      await JSRuntime.InvokeAsync<object>(JsFunctionName, "init", aState);
    }
  }

  public async Task InitAsync()
  {
    if (IsInitialized) return;

    Logger.LogDebug(EventIds.ReduxDevToolsInterop_Initializing, "Init ReduxDevToolsInterop");
    const string ReduxDevToolsFactoryName = "ReduxDevToolsFactory";
    IsEnabled = await JSRuntime.InvokeAsync<bool>(ReduxDevToolsFactoryName, ReduxDevToolsOptions);

    if (IsEnabled)
      await DispatchInitAsync(Store.GetSerializableState());

    IsInitialized = true;
  }
}
