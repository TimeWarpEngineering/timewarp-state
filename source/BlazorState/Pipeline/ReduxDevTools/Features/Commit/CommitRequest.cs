namespace BlazorState.Pipeline.ReduxDevTools
{
  using MediatR;

  internal class CommitRequest : DispatchRequest<CommitRequest.PayloadClass>, IRequest, IReduxRequest
  {
    internal class PayloadClass
    {
      public string Type { get; set; }
    }
  }
}