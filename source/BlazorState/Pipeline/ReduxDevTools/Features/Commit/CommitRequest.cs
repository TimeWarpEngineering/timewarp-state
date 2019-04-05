namespace BlazorState.Pipeline.ReduxDevTools.Features.Commit
{
  using MediatR;

  internal class CommitRequest : DispatchRequest<Payload>, IRequest, IReduxRequest { }
  internal class Payload
  {
    public string Type { get; set; }
  }
}