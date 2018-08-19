namespace BlazorState.Behaviors.ReduxDevTools.Features.JumpToState
{
  using BlazorState.Features.JavaScriptInterop;
  using MediatR;
  using Microsoft.AspNetCore.Blazor;
  using Microsoft.JSInterop;

  internal class JumpToStateRequest : IRequest, IReduxRequest
  {
    public JumpToStateRequest() { }  //Needed for De-serialize below

    public JumpToStateRequest(string aRequestAsJson) : this()
    {
      JsonRequest =
        Json.Deserialize<JsonRequest<DispatchRequest<JumpToStateRequest>>>(aRequestAsJson);

      Type = JsonRequest.Payload.Type;
      ActionId = JsonRequest.Payload.Payload.ActionId;
    }

    public int ActionId { get; set; }
    public JsonRequest<DispatchRequest<JumpToStateRequest>> JsonRequest { get; }
    public string Type { get; set; }
  }
}