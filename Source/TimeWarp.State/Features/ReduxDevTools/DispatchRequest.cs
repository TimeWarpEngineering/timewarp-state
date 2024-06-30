namespace TimeWarp.Features.ReduxDevTools;

internal class DispatchRequest<TPayload>
{
  protected DispatchRequest(int id, TPayload payload, string source, string state, string type)
  {
    Id = id;
    Payload = payload;
    Source = source;
    State = state;
    Type = type;
  }
  public int Id { get; set; }
  public TPayload Payload { get; set; }
  public string Source { get; set; }
  public string State { get; set; }
  public string Type { get; set; }
}
