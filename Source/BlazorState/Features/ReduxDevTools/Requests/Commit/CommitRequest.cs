namespace TimeWarp.Features.ReduxDevTools;

[UsedImplicitly]
internal class CommitRequest : DispatchRequest<CommitRequest.PayloadClass>, IRequest, IReduxRequest
{
  [UsedImplicitly]
  internal class PayloadClass
  {
    public PayloadClass(string type) 
    {
      Type = type;
    }
    public string Type { get; }
  }

  public CommitRequest(int id, PayloadClass payload, string source, string state, string type) : base(id, payload, source, state, type) {}
}
