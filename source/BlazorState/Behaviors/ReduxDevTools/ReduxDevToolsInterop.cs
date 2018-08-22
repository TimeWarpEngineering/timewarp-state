namespace BlazorState.Behaviors.ReduxDevTools
{
  using System.Threading.Tasks;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  public class ReduxDevToolsInterop
  {
    private const string JsFunctionName = "reduxDevTools.ReduxDevToolsDispatch";

    public ReduxDevToolsInterop(ILogger<ReduxDevToolsInterop> aLogger)
    {
      Logger = aLogger;
    }

    public bool IsEnabled { get; set; }
    private ILogger Logger { get; }

    public async Task InitAsync()
    {
      const string ReduxDevToolsFactoryName = "reduxDevToolsFactory";
      IsEnabled = await JSRuntime.Current.InvokeAsync<bool>(ReduxDevToolsFactoryName);
      // We could send in the Store.GetSerializeState but it will be empty
      if (IsEnabled)
        DispatchInit(string.Empty);
    }

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
  }
}