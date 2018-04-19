namespace BlazorState.Behaviors.ReduxDevTools.Features.JumpToState
{
  using BlazorState.Behaviors.JavaScriptInterop;
  using MediatR;
  using Microsoft.AspNetCore.Blazor;

  public class Request : IRequest, IReduxRequest
  {
    public Request() { }  //Needed for Deserialize below

    public Request(string aRequestAsJson) : this()
    {
      JsonRequest =
        JsonUtil.Deserialize<JsonRequest<DispatchRequest<Request>>>(aRequestAsJson);

      Type = JsonRequest.Payload.Type;
      ActionId = JsonRequest.Payload.Payload.ActionId;
    }

    public int ActionId { get; set; }
    public JsonRequest<DispatchRequest<Request>> JsonRequest { get; }
    public string Type { get; set; }
  }
}