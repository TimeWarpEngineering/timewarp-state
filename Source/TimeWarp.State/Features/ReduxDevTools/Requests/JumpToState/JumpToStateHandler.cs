namespace TimeWarp.Features.ReduxDevTools;

internal class JumpToStateHandler : IRequestHandler<JumpToStateRequest>
{
  private readonly ILogger Logger;
  private readonly IReduxDevToolsStore Store;
  private readonly Subscriptions Subscriptions;

  public JumpToStateHandler
  (
    ILogger<JumpToStateHandler> logger,
    IReduxDevToolsStore store,
    Subscriptions subscriptions
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.JumpToStateHandler_Initializing, "constructor");
    Store = store;
    Subscriptions = subscriptions;
  }

  public Task Handle(JumpToStateRequest jumpToStateRequest, CancellationToken cancellationToken)
  {
    Logger.LogDebug
    (
      EventIds.JumpToStateHandler_RequestReceived,
      "Received Id:{aJumpToStateRequest_Id} State:{aRequest_State}",
      jumpToStateRequest.Id,
      jumpToStateRequest.State
    );

    Store.LoadStatesFromJson(jumpToStateRequest.State);
    Subscriptions.ReRenderSubscribers<IDevToolsComponent>();

    Logger.LogDebug
    (
      EventIds.JumpToStateHandler_RequestHandled,
      "Handled Id:{aJumpToStateRequest_Id}",
      jumpToStateRequest.Id
    );
    return Task.CompletedTask;
  }
}
