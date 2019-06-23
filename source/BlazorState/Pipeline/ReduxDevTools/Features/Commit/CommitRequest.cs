namespace BlazorState.Pipeline.ReduxDevTools
{
  using MediatR;

  internal class CommitRequest : DispatchRequest<CommitRequest.Payload>, IRequest, IReduxRequest
  {
    internal class Payload
    {
      public string Type { get; set; }
    }
  }
}