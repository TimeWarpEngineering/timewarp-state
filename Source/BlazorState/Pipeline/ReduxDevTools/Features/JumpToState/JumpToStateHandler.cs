namespace BlazorState.Pipeline.ReduxDevTools
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class JumpToStateHandler : RequestHandler<JumpToStateRequest>
  {
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

    private Subscriptions Subscriptions { get; }
    private ILogger Logger { get; }
    private IReduxDevToolsStore Store { get; }

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