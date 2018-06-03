namespace BlazorState.Behaviors.ReduxDevTools.Features.Start
{
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class StartHandler : IRequestHandler<StartRequest>
  {
    public StartHandler(
            ILogger<StartHandler> aLogger,
      IStore aStore,
      ReduxDevToolsInterop aReduxDevToolsInterop)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().FullName} constructor");
      Store = aStore;
      ReduxDevToolsInterop = aReduxDevToolsInterop;
    }

    private ILogger Logger { get; }
    private ReduxDevToolsInterop ReduxDevToolsInterop { get; }
    private IStore Store { get; }

    public Task Handle(StartRequest aRequest, CancellationToken aCancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}