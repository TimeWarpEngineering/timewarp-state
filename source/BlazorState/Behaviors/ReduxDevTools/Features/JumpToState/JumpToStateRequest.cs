namespace BlazorState.Behaviors.ReduxDevTools.Features.JumpToState
{
  using BlazorState.Features.JavaScriptInterop;
  using MediatR;
  using Microsoft.AspNetCore.Blazor;

  internal class JumpToStateRequest : IRequest, IReduxRequest
  {
    public JumpToStateRequest() { }  //Needed for De-serialize below

    public JumpToStateRequest(string aRequestAsJson) : this()
    {
      JsonRequest =
        JsonUtil.Deserialize<JsonRequest<DispatchRequest<JumpToStateRequest>>>(aRequestAsJson);

      Type = JsonRequest.Payload.Type;
      ActionId = JsonRequest.Payload.Payload.ActionId;
    }

    public int ActionId { get; set; }
    public JsonRequest<DispatchRequest<JumpToStateRequest>> JsonRequest { get; }
    public string Type { get; set; }
  }
}