namespace BlazorState.Behaviors.ReduxDevTools
{
  public class DispatchRequest<TPayload>
  {
    public int Id { get; set; }
    public TPayload Payload { get; set; }
    public string Source { get; set; }
    public string State { get; set; }
    public string Type { get; set; }
  }
}