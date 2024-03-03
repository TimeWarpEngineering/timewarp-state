namespace TimeWarp.Features.ReduxDevTools;

[UsedImplicitly]
internal class CommitRequest : DispatchRequest<CommitRequest.PayloadClass>, IRequest, IReduxRequest
{
  [UsedImplicitly]
  internal class PayloadClass
  {
    public string Type { get; set; }
  }
}
