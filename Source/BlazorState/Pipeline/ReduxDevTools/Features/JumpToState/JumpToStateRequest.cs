namespace BlazorState.Pipeline.ReduxDevTools;

internal class JumpToStateRequest : DispatchRequest<JumpToStateRequest.PayloadClass>, IRequest, IReduxRequest
{

  internal class PayloadClass
  {
    public int ActionId { get; set; }
    public string Type { get; set; }

  }
}
