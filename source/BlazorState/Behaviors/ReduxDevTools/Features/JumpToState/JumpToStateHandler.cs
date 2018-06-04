namespace BlazorState.Behaviors.ReduxDevTools.Features.JumpToState
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class JumpToStateHandler : RequestHandler<JumpToStateRequest>
  {
    public JumpToStateHandler(
      ILogger<JumpToStateHandler> aLogger,
      IStore aStore,
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
    private IStore Store { get; }

    protected override void Handle(JumpToStateRequest aRequest)
    {
      Logger.LogDebug($"{GetType().FullName}");
      Logger.LogDebug($"{aRequest.JsonRequest.Payload.State}");
      Store.LoadStatesFromJson(aRequest.JsonRequest.Payload.State);
      ComponentRegistry.ReRenderAll();
    }
  }
}