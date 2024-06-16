namespace TimeWarp.Features.ReduxDevTools;

internal class JumpToStateRequest : DispatchRequest<JumpToStateRequest.PayloadClass>, IRequest, IReduxRequest
{
  public JumpToStateRequest(int id, PayloadClass payload, string source, string state, string type) : base(id, payload, source, state, type) {}

  internal class PayloadClass
  {
    public int ActionId { get; set; }
    public string Type { get; set; }
    public PayloadClass(string type)
    {
      Type = type;
    }
  }
}
