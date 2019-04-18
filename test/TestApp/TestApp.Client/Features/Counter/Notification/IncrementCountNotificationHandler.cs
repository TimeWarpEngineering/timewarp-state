namespace TestApp.Client.Features.Counter
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  using MediatR;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Pipeline.NotificationPostProcessor;

  internal class IncrementCountNotificationHandler
    : INotificationHandler<PostPipelineNotification<IncrementCounterAction, CounterState>>
  {
    public Task Handle(PostPipelineNotification<IncrementCounterAction, CounterState> aNotification, CancellationToken aCancellationToken)
    {
      Console.WriteLine(aNotification.Request.GetType().Name);
      Console.WriteLine("IncrementCountNotificationHandler handled");
      return Task.CompletedTask;
    }
  }
}
