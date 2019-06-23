namespace BlazorState.Pipeline.ReduxDevTools
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class CommitHandler : RequestHandler<CommitRequest>
  {
    public CommitHandler(
      ILogger<CommitHandler> aLogger,
      IReduxDevToolsStore aStore,
      ReduxDevToolsInterop aReduxDevToolsInterop)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().FullName} constructor");
      Store = aStore;
      ReduxDevToolsInterop = aReduxDevToolsInterop;
    }

    private ILogger Logger { get; }
    private ReduxDevToolsInterop ReduxDevToolsInterop { get; }
    private IReduxDevToolsStore Store { get; }

    protected override void Handle(CommitRequest aRequest)
    {
      Logger.LogDebug($"{GetType().FullName}");
      Logger.LogDebug($"{aRequest.Type}");

      ReduxDevToolsInterop.DispatchInit(Store.GetSerializableState());
    }
  }
}