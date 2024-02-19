namespace Test.App.Client.Features.Application;

using BlazorState.Pipeline.State;

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
      ExceptionNotification aExceptionNotification,
      CancellationToken aCancellationToken
    )
    {
      Logger.LogWarning($"aExceptionNotification.Exception.Message: {aExceptionNotification.Exception.Message}");
      ApplicationState.ExceptionMessage = aExceptionNotification.Exception.Message;
      return Task.CompletedTask;
    }
  }
}
