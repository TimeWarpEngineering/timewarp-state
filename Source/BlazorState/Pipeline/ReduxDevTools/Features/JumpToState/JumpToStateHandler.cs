namespace BlazorState.Pipeline.ReduxDevTools
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class JumpToStateHandler : RequestHandler<JumpToStateRequest>
  {
    private readonly ILogger Logger;

    private readonly IReduxDevToolsStore Store;

    private readonly Subscriptions Subscriptions;

    public JumpToStateHandler(
                  ILogger<JumpToStateHandler> aLogger,
      IReduxDevToolsStore aStore,
      Subscriptions aSubscriptions)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().FullName} constructor");
      Store = aStore;
      Subscriptions = aSubscriptions;
    }

    protected override void Handle(JumpToStateRequest aRequest)
    {
      Logger.LogDebug($"Type:{GetType().FullName}");
      Logger.LogDebug($"State: {aRequest.State}");
      Store.LoadStatesFromJson(aRequest.State);
      Logger.LogDebug($"After LoadStatesFromJson");
      Subscriptions.ReRenderSubscribers<IDevToolsComponent>();
      Logger.LogDebug($"After ReRenderAll");
    }
  }
}