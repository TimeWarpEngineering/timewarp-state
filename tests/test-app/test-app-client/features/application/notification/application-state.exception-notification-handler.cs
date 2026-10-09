#region Purpose
// Records the message of each ExceptionNotification in ApplicationState.ExceptionMessage.
#endregion

#region Design
// Nested in ApplicationState so it can set the private setter. It writes the state directly, not through an action,
// and logs a warning; the state transaction tests read ExceptionMessage to confirm a failed action was published.
#endregion

namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  internal class ExceptionNotificationHandler
  (
    ILogger<ExceptionNotificationHandler> Logger,
    IStore Store
  ) : INotificationHandler<ExceptionNotification>
  {
    private readonly ILogger Logger = Logger;
    private ApplicationState ApplicationState => Store.GetState<ApplicationState>();

    public Task Handle
    (
      ExceptionNotification exceptionNotification,
      CancellationToken cancellationToken
    )
    {
      Logger.LogWarning("exceptionNotification.Exception.Message: {ExceptionMessage}", exceptionNotification.Exception.Message);
      ApplicationState.ExceptionMessage = exceptionNotification.Exception.Message;
      return Task.CompletedTask;
    }
  }
}
