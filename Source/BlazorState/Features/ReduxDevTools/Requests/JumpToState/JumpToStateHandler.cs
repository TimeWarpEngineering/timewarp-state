namespace TimeWarp.Features.ReduxDevTools;

internal class JumpToStateHandler : IRequestHandler<JumpToStateRequest>
{
  private readonly ILogger Logger;
  private readonly IReduxDevToolsStore Store;
  private readonly Subscriptions Subscriptions;

  public JumpToStateHandler
  (
    ILogger<JumpToStateHandler> aLogger,
    IReduxDevToolsStore aStore,
    Subscriptions aSubscriptions
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.JumpToStateHandler_Initializing, "constructor");
    Store = aStore;
    Subscriptions = aSubscriptions;
  }

  public Task Handle(JumpToStateRequest aJumpToStateRequest, CancellationToken aCancellationToken)
  {
    Logger.LogDebug
    (
      EventIds.JumpToStateHandler_RequestReceived,
      "Recieved Id:{aJumpToStateRequest_Id} State:{aRequest_State}",
      aJumpToStateRequest.Id,
      aJumpToStateRequest.State
    );

    Store.LoadStatesFromJson(aJumpToStateRequest.State);
    Subscriptions.ReRenderSubscribers<IDevToolsComponent>();

    Logger.LogDebug
    (
      EventIds.JumpToStateHandler_RequestHandled,
      "Handled Id:{aJumpToStateRequest_Id}",
      aJumpToStateRequest.Id
    );
    return Task.CompletedTask;
  }
}
