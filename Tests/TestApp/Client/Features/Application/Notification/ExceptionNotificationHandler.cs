namespace TestApp.Client.Features.Application
{
  using BlazorState;
  using BlazorState.Pipeline.State;
  using MediatR;
  using Microsoft.Extensions.Logging;
  using System.Threading;
  using System.Threading.Tasks;

  internal partial class ApplicationState
  {
    internal class ExceptionNotificationHandler
      : INotificationHandler<ExceptionNotification>
    {
      private readonly ILogger Logger;
      private readonly IStore Store;
      private ApplicationState ApplicationState => Store.GetState<ApplicationState>();

      public ExceptionNotificationHandler
      (
        ILogger<ExceptionNotificationHandler> aLogger,
        IStore aStore
      )
      {
        Logger = aLogger;
        Store = aStore;
      }

      public Task Handle
      (
        ExceptionNotification aExceptionNotification,
        CancellationToken aCancellationToken
      )
      {
        Logger.LogWarning($"aExceptionNotification.Exception.Message: {aExceptionNotification.Exception.Message}");
        ApplicationState.ExceptionMessage = aExceptionNotification.Exception.Message;
        return Unit.Task;
      }
    }
  }
}