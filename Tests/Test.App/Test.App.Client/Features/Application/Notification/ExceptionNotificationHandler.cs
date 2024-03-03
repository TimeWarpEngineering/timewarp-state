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
      ExceptionNotification aExceptionNotification,
      CancellationToken aCancellationToken
    )
    {
      Logger.LogWarning("aExceptionNotification.Exception.Message: {ExceptionMessage}", aExceptionNotification.Exception.Message);
      ApplicationState.ExceptionMessage = aExceptionNotification.Exception.Message;
      return Task.CompletedTask;
    }
  }
}
