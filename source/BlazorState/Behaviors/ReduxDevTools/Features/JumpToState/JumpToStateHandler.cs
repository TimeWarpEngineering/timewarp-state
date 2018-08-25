namespace BlazorState.Behaviors.ReduxDevTools.Features.JumpToState
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class JumpToStateHandler : RequestHandler<JumpToStateRequest>
  {
    public JumpToStateHandler(
      ILogger<JumpToStateHandler> aLogger,
      IReduxDevToolsStore aStore,
      ReduxDevToolsInterop aReduxDevToolsInterop,
      ComponentRegistry aComponentRegistry)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().FullName} constructor");
      Store = aStore;
      ReduxDevToolsInterop = aReduxDevToolsInterop;
      ComponentRegistry = aComponentRegistry;
    }

    private ComponentRegistry ComponentRegistry { get; }
    private ILogger Logger { get; }
    private ReduxDevToolsInterop ReduxDevToolsInterop { get; }
    private IReduxDevToolsStore Store { get; }

    protected override void Handle(JumpToStateRequest aRequest)
    {
      Logger.LogDebug($"Type:{GetType().FullName}");
      Logger.LogDebug($"State: {aRequest.JsonRequest.Payload.State}");
      Store.LoadStatesFromJson(aRequest.JsonRequest.Payload.State);
      Logger.LogDebug($"After LoadStatesFromJson");
      ComponentRegistry.ReRenderAll();
      Logger.LogDebug($"After ReRenderAll");
    }
  }
}