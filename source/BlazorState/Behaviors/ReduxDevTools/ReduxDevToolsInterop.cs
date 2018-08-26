namespace BlazorState.Behaviors.ReduxDevTools
{
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  public class ReduxDevToolsInterop
  {
    private const string JsFunctionName = "reduxDevTools.ReduxDevToolsDispatch";

    public ReduxDevToolsInterop(
      ILogger<ReduxDevToolsInterop> aLogger,
      IReduxDevToolsStore aStore)
    {

      Logger = aLogger;
      Store = aStore;
    }

    public bool IsEnabled { get; set; }
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
      const string ReduxDevToolsFactoryName = "ReduxDevToolsFactory";
      IsEnabled = await JSRuntime.Current.InvokeAsync<bool>(ReduxDevToolsFactoryName);

      if (IsEnabled)
        DispatchInit(Store.GetSerializableState());
    }
  }
}