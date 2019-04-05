namespace TestApp.Client.Pipeline.NotificationPreProcessor
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;
  using MediatR.Pipeline;

  internal class PrePipelineNotificationRequestPreProcessor <TRequest> : IRequestPreProcessor<TRequest>
  {
    public PrePipelineNotificationRequestPreProcessor(IMediator aMediator)
    {
      Mediator = aMediator;
    }

    private IMediator Mediator { get; }

    public async Task Process(TRequest aRequest, CancellationToken aCancellationToken)
    {
      var notification = new PrePipelineNotification<TRequest>
      {
        Request = aRequest,
      };

      Console.WriteLine("PrePipelineNotificationRequestPreProcessor");
      await Mediator.Publish(notification);
    }
  }
}