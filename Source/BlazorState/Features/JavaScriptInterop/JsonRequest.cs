namespace BlazorState.Features.JavaScriptInterop
{
  public class BaseJsonRequest
  {
    public string RequestType { get; set; }
  }

  public class JsonRequest<TPayload>
  {
    public TPayload Payload { get; set; }
    public string RequestType { get; set; }
  }
}