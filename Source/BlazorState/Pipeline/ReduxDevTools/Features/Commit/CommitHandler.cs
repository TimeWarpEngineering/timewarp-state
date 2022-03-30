namespace BlazorState.Pipeline.ReduxDevTools;

using BlazorState;
using MediatR;
using System.Threading;

internal class CommitHandler : IRequestHandler<CommitRequest>
{
  private readonly ReduxDevToolsInterop ReduxDevToolsInterop;

  private readonly IReduxDevToolsStore Store;

  public CommitHandler
  (
    IReduxDevToolsStore aStore,
    ReduxDevToolsInterop aReduxDevToolsInterop
  )
  {
    Store = aStore;
    ReduxDevToolsInterop = aReduxDevToolsInterop;
  }

  public async Task<Unit> Handle(CommitRequest aRequest, CancellationToken aCancellationToken)
  {
    await ReduxDevToolsInterop.DispatchInitAsync(Store.GetSerializableState());
    return Unit.Value;
  }
}
