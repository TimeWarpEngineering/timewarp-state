namespace TestApp.Client.Pipeline.NotificationPostProcessor
{
  using System;
  using System.Threading.Tasks;
  using MediatR;
  using MediatR.Pipeline;

  internal class PostPipelineNotificationRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  {
    public PostPipelineNotificationRequestPostProcessor(IMediator aMediator)
    {
      Mediator = aMediator;
    }

    private IMediator Mediator { get; }

    public async Task Process(TRequest aRequest, TResponse aResponse)
    {
      var notification = new PostPipelineNotification<TRequest, TResponse>
      {
        Request = aRequest,
        Response = aResponse
      };

      Console.WriteLine("PostPipelineNotificationRequestPostProcessor");
      await Mediator.Publish(notification);
    }
  }
}