namespace TimeWarp.Features.ReduxDevTools;

public class ReduxDevToolsInterop
{
  private const string JsFunctionName = "reduxDevTools.ReduxDevToolsDispatch";

  private readonly IJSRuntime JsRuntime;
  private readonly ILogger Logger;
  private readonly IReduxDevToolsStore Store;
  private readonly ReduxDevToolsOptions ReduxDevToolsOptions;
  
  private bool IsInitialized;
  private bool IsEnabled;

  public ReduxDevToolsInterop
  (
    ILogger<ReduxDevToolsInterop> logger,
    IReduxDevToolsStore store,
    ReduxDevToolsOptions reduxDevToolsOptions,
    IJSRuntime jsRuntime
  )
  {
    Logger = logger;
    Store = store;
    ReduxDevToolsOptions = reduxDevToolsOptions;
    JsRuntime = jsRuntime;
  }

  public async Task DispatchAsync<TRequest>(TRequest request, object state, string? stackTrace)
  where TRequest : notnull
  {
    if (IsEnabled)
    {
      var reduxAction = new ReduxAction(request);

      Logger.LogDebug
      (
        EventIds.ReduxDevToolsInterop_DispatchingRequest,
        "dispatching request.GetType().FullName:{TypeFullName} to ReduxDevTools",
        request.GetType().FullName
      );

      await JsRuntime.InvokeAsync<object>(JsFunctionName, reduxAction, state, stackTrace);
    }
  }

  public async Task DispatchInitAsync(object state)
  {
    if (IsEnabled)
    {
      Logger.LogDebug(EventIds.ReduxDevToolsInterop_DispatchingInit, "dispatching init to ReduxDevTools");
      await JsRuntime.InvokeAsync<object>(JsFunctionName, "init", state);
    }
  }

  public async Task InitAsync()
  {
    if (IsInitialized) return;

    Logger.LogDebug(EventIds.ReduxDevToolsInterop_Initializing, "Init ReduxDevToolsInterop");
    const string reduxDevToolsFactoryName = "ReduxDevToolsFactory";
    IsEnabled = await JsRuntime.InvokeAsync<bool>(reduxDevToolsFactoryName, ReduxDevToolsOptions);

    if (IsEnabled)
      await DispatchInitAsync(Store.GetSerializableState());

    IsInitialized = true;
  }
}
