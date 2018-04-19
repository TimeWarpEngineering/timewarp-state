namespace BlazorState.Behaviors.ReduxDevTools.Features.JumpToState
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Store;
  using MediatR;
  using Microsoft.Extensions.Logging;

  public class Handler : IRequestHandler<Request>
  {
    public Handler(
      ILogger<Handler> aLogger,
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

    public Task Handle(Request aRequest, CancellationToken aCancellationToken)
    {
      Logger.LogDebug($"{GetType().FullName}");
      Logger.LogDebug($"{aRequest.JsonRequest.Payload.State}");
      Store.LoadStatesFromJson(aRequest.JsonRequest.Payload.State);
      ComponentRegistry.ReRenderAll();
      return Task.CompletedTask;
    }
  }
}