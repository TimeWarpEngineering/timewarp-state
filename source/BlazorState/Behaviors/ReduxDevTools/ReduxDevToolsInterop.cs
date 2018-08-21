namespace BlazorState.Behaviors.ReduxDevTools
{
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  internal class ReduxDevToolsInterop
  {
    private const string JsFunctionName = "reduxDevTools.ReduxDevToolsDispatch";

    public ReduxDevToolsInterop(ILogger<ReduxDevToolsInterop> aLogger)
    {
      Logger = aLogger;
    }

    public bool IsEnabled { get; set; }
    private ILogger Logger { get; }

    public void Dispatch<TRequest>(TRequest aRequest, object state)
    {
      if (IsEnabled)
      {
        Logger.LogDebug($"{GetType().Name}: {nameof(this.Dispatch)}");
        Logger.LogDebug($"{GetType().Name}: aRequest.GetType().FullName:{aRequest.GetType().FullName}");
        var reduxAction = new ReduxAction(aRequest);
        JSRuntime.Current.InvokeAsync<object>(JsFunctionName, reduxAction, state);
      }
    }

    public void DispatchInit(object state)
    {
      if (IsEnabled)
        JSRuntime.Current.InvokeAsync<object>(JsFunctionName, "init", state);
    }
  }
}