#region Purpose
// Handles the Start message Redux DevTools sends once on startup.
#endregion

#region Design
// The constructor logs EventIds.StartHandler_Initializing (500). Handle logs
// EventIds.StartHandler_RequestReceived (501) and does no other work. StartHandler_RequestHandled (502) is unused.
// Having a handler lets the Start request dispatch without error.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Redux Devtools will send the Request once on startup
/// </summary>
/// <remarks>
/// The constructor logs <see cref="EventIds.StartHandler_Initializing"/>.
/// <see cref="Handle"/> logs <see cref="EventIds.StartHandler_RequestReceived"/> and performs no other work.
/// </remarks>
public class StartHandler : IRequestHandler<StartRequest>
{
  private readonly ILogger Logger;

  public StartHandler
  (
    ILogger<StartHandler> logger
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.StartHandler_Initializing, "constructing");
  }

  /// <summary>
  /// Logs <see cref="EventIds.StartHandler_RequestReceived"/> and completes.
  /// </summary>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>A completed task.</returns>
  public Task Handle(StartRequest request, CancellationToken cancellationToken)
  {
    Logger.LogDebug(EventIds.StartHandler_RequestReceived, "received");
    return Task.CompletedTask;
  }
}
