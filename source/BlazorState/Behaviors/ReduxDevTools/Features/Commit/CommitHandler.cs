namespace BlazorState.Behaviors.ReduxDevTools.Features.Commit
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class CommitHandler : RequestHandler<CommitRequest>
  {
    public CommitHandler(
      ILogger<CommitHandler> aLogger,
      IStore aStore,
      ReduxDevToolsInterop aReduxDevToolsInterop)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().FullName} constructor");
      Store = aStore;
      ReduxDevToolsInterop = aReduxDevToolsInterop;
    }

    private ILogger Logger { get; }
    private ReduxDevToolsInterop ReduxDevToolsInterop { get; }
    private IStore Store { get; }

    protected override void Handle(CommitRequest aRequest)
    {
      Logger.LogDebug($"{GetType().FullName}");
      Logger.LogDebug($"{aRequest.Type}");

      ReduxDevToolsInterop.DispatchInit(Store.GetSerializableState());
    }
  }
}