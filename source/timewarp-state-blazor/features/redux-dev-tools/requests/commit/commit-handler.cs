#region Purpose
// Handles the Redux DevTools Commit message by re-sending the current serializable state as the new init state.
#endregion

#region Design
// A plain IRequestHandler (not an action) reached from JavaScript through JsonRequestHandler. That path is only
// allow-listed when UseReduxDevTools is called.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

public class CommitHandler : IRequestHandler<CommitRequest>
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
