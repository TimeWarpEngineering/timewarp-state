namespace BlazorState.Pipeline.ReduxDevTools
{
  using MediatR;

  internal class JumpToStateRequest : DispatchRequest<JumpToStateRequest.Payload>, IRequest, IReduxRequest
  {

    internal class Payload
    {
      public int ActionId { get; set; }
      public string Type { get; set; }

    }
  }
}