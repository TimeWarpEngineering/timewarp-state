namespace BlazorState.Pipeline.ReduxDevTools
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class CommitHandler : RequestHandler<CommitRequest>
  {
    private readonly ILogger Logger;

    private readonly ReduxDevToolsInterop ReduxDevToolsInterop;

    private readonly IReduxDevToolsStore Store;

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

    protected override void Handle(CommitRequest aRequest)
    {
      Logger.LogDebug($"{GetType().FullName}");
      Logger.LogDebug($"{aRequest.Type}");

      ReduxDevToolsInterop.DispatchInit(Store.GetSerializableState());
    }
  }
}