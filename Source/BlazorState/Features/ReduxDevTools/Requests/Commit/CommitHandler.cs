namespace TimeWarp.Features.ReduxDevTools;

internal class CommitHandler : IRequestHandler<CommitRequest>
{
  private readonly ILogger Logger;
  private readonly IReduxDevToolsStore Store;
  private readonly ReduxDevToolsInterop ReduxDevToolsInterop;

  public CommitHandler
  (
    ILogger<CommitHandler> logger,
    IReduxDevToolsStore store,
    ReduxDevToolsInterop reduxDevToolsInterop
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.CommitHandler_Initializing, "constructor");
    Store = store;
    ReduxDevToolsInterop = reduxDevToolsInterop;
  }

  public async Task Handle(CommitRequest commitRequest, CancellationToken cancellationToken)
  {
    Logger.LogDebug
    (
      EventIds.CommitHandler_RequestReceived,
      "Received Id:{aJumpToStateRequest_Id} State:{aRequest_State}",
      commitRequest.Id,
      commitRequest.State
    );

    await ReduxDevToolsInterop.DispatchInitAsync(Store.GetSerializableState());

    Logger.LogDebug
    (
      EventIds.JumpToStateHandler_RequestReceived,
      "Received Id:{aJumpToStateRequest_Id}",
      commitRequest.Id
    );
  }
}
