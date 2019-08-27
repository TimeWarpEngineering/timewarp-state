namespace BlazorState.Pipeline.ReduxDevTools
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class StartHandler : RequestHandler<StartRequest>
  {
    public StartHandler(
      ILogger<StartHandler> aLogger
      //IStore aStore,
      //ReduxDevToolsInterop aReduxDevToolsInterop
      )
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().FullName} constructor");
      //Store = aStore;
      //ReduxDevToolsInterop = aReduxDevToolsInterop;
    }

    private ILogger Logger { get; }
    //private ReduxDevToolsInterop ReduxDevToolsInterop { get; }
    //private IStore Store { get; }

    protected override void Handle(StartRequest aRequest)
    {
      // Does nothing currently
    }
  }
}