namespace BlazorState.Behaviors.ReduxDevTools
{
  using Microsoft.AspNetCore.Blazor.Browser.Interop;
  using Microsoft.Extensions.Logging;

  public class ReduxDevToolsInterop
  {
    private const string jsFunctionName = "ReduxDevToolsDispatch";

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
        RegisteredFunction.Invoke<object>(jsFunctionName, reduxAction, state);
      }
    }

    public void DispatchInit(object state)
    {
      if (IsEnabled)
        RegisteredFunction.Invoke<object>(jsFunctionName, "init", state);
    }
  }
}