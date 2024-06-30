namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  [UsedImplicitly]
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
      Logger.LogWarning("aExceptionNotification.Exception.Message: {ExceptionMessage}", exceptionNotification.Exception.Message);
      ApplicationState.ExceptionMessage = exceptionNotification.Exception.Message;
      return Task.CompletedTask;
    }
  }
}
