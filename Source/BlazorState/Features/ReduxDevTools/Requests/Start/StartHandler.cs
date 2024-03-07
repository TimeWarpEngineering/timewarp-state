namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Redux Devtools will send the Request once on startup
/// </summary>
/// <remarks>currently we do nothing at start up other than log</remarks>
internal class StartHandler : IRequestHandler<StartRequest>
{
  private readonly ILogger Logger;

  public StartHandler
  (
    ILogger<StartHandler> logger
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.JumpToStateHandler_RequestHandled, "constructing");
  }

  /// <summary>
  /// Currently does nothing
  /// </summary>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task Handle(StartRequest request, CancellationToken cancellationToken) =>
    Task.CompletedTask;
}
