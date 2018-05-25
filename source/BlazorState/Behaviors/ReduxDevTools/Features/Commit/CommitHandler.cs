namespace BlazorState.Behaviors.ReduxDevTools.Features.Commit
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  public class CommitHandler : IRequestHandler<CommitRequest>
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

    public Task Handle(CommitRequest aRequest, CancellationToken aCancellationToken)
    {
      Logger.LogDebug($"{GetType().FullName}");
      Logger.LogDebug($"{aRequest.Type}");

      ReduxDevToolsInterop.DispatchInit(Store.GetSerializableState());
      return Task.CompletedTask;
    }
  }
}