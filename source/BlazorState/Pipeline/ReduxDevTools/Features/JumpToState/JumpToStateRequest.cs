namespace BlazorState.Pipeline.ReduxDevTools.Features.JumpToState
{
  using MediatR;

  internal class JumpToStateRequest : DispatchRequest<Payload>, IRequest, IReduxRequest { }

  internal class Payload
  {
    public int ActionId { get; set; }
    public string Type { get; set; }

  }
}