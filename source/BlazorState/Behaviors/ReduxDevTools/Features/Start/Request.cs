namespace BlazorState.Behaviors.ReduxDevTools.Features.Start
{
  using BlazorState.Behaviors.JavaScriptInterop;
  using MediatR;
  using Microsoft.AspNetCore.Blazor;

  /// <summary>
  /// Request received from Redux Dev Tools when one presses the Start Button.
  /// </summary>
  public class Request : IRequest, IReduxRequest
  {
    public Request() { }  //Needed for Deserialize below

    public Request(string aRequestAsJson) : this()
    {
      JsonRequest<Request> jsonRequest =
        JsonUtil.Deserialize<JsonRequest<Request>>(aRequestAsJson);

      Type = jsonRequest.Payload.Type;
      Source = jsonRequest.Payload.Source;
    }

    public string Source { get; set; }
    public string Type { get; set; }
  }
}