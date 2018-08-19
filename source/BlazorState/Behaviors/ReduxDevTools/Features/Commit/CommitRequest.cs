namespace BlazorState.Behaviors.ReduxDevTools.Features.Commit
{
  using BlazorState.Features.JavaScriptInterop;
  using MediatR;
  using Microsoft.AspNetCore.Blazor;
  using Microsoft.JSInterop;

  internal class CommitRequest : IRequest, IReduxRequest
  {
    public CommitRequest() { }  //Needed for Deserialize below

    public CommitRequest(string aRequestAsJson) : this()
    {
      JsonRequest =
        Json.Deserialize<JsonRequest<DispatchRequest<CommitRequest>>>(aRequestAsJson);

      Type = JsonRequest.Payload.Type;
    }

    public JsonRequest<DispatchRequest<CommitRequest>> JsonRequest { get; }
    public string Type { get; set; }
  }
}