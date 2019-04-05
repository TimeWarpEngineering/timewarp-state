//namespace TestApp.Client.Features.EventStream
//{
//  using System;
//  using System.Threading;
//  using System.Threading.Tasks;

//  using MediatR;
//  using TestApp.Client.Features.Counter;
//  using TestApp.Client.Pipeline.NotificationPostProcessor;

//  internal class PostPipelineNotificationHandler<TRequest, TResponse>
//    : INotificationHandler<PostPipelineNotification<TRequest, TResponse>>
//  {
//    public Task Handle(PostPipelineNotification<TRequest, TResponse> aNotification, CancellationToken aCancellationToken)
//    {
//      Console.WriteLine(aNotification.Request.GetType().Name);
//      Console.WriteLine("PostPipelineNotificationHandler handled");
//      return Task.CompletedTask;
//    }
//  }
//}
