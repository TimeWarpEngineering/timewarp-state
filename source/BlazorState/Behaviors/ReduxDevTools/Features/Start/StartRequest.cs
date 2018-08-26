namespace BlazorState.Behaviors.ReduxDevTools.Features.Start
{
  using BlazorState.Features.JavaScriptInterop;
  using MediatR;
  using Microsoft.JSInterop;

  /// <summary>
  /// Request received from Redux Dev Tools when one presses the Start Button.
  /// </summary>
  internal class StartRequest : IRequest, IReduxRequest
  {
    public StartRequest() { }  //Needed for De serialize below

    public StartRequest(string aRequestAsJson) : this()
    {
      JsonRequest<StartRequest> jsonRequest =
        Json.Deserialize<JsonRequest<StartRequest>>(aRequestAsJson);

      Type = jsonRequest.Payload.Type;
      Source = jsonRequest.Payload.Source;
    }

    public string Source { get; set; }
    public string Type { get; set; }
  }
}