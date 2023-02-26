namespace BlazorState.Pipeline.ReduxDevTools;

using BlazorState;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;

internal class CommitHandler : IRequestHandler<CommitRequest>
{
  private readonly ILogger Logger;
  private readonly IReduxDevToolsStore Store;
  private readonly ReduxDevToolsInterop ReduxDevToolsInterop;

  public CommitHandler
  (
    ILogger<CommitHandler> aLogger,
    IReduxDevToolsStore aStore,
    ReduxDevToolsInterop aReduxDevToolsInterop
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.CommitHandler_Initializing, "constructor");
    Store = aStore;
    ReduxDevToolsInterop = aReduxDevToolsInterop;
  }

  public async Task Handle(CommitRequest aCommitRequest, CancellationToken aCancellationToken)
  {
    Logger.LogDebug
    (
      EventIds.CommitHandler_RequestReceived,
      "Received Id:{aJumpToStateRequest_Id} State:{aRequest_State}",
      aCommitRequest.Id,
      aCommitRequest.State
    );

    await ReduxDevToolsInterop.DispatchInitAsync(Store.GetSerializableState());

    Logger.LogDebug
    (
      EventIds.JumpToStateHandler_RequestReceived,
      "Received Id:{aJumpToStateRequest_Id}",
      aCommitRequest.Id
    );
  }
}
