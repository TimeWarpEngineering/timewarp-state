namespace BlazorState.Pipeline.ReduxDevTools;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

/// <summary>
/// Redux Devtools will send the Request once on startup
/// </summary>
/// <remarks>currently we do nothing at start up other than log</remarks>
internal class StartHandler : IRequestHandler<StartRequest>
{
  private readonly ILogger Logger;

  public StartHandler
  (
    ILogger<StartHandler> aLogger
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.JumpToStateHandler_RequestHandled, "constructing");
  }

  /// <summary>
  /// Currently does nothing
  /// </summary>
  /// <param name="aRequest"></param>
  /// <param name="aCancellationToken"></param>
  /// <returns></returns>
  public Task Handle(StartRequest aRequest, CancellationToken aCancellationToken) =>
    Task.CompletedTask;
}
