#region Purpose
// Shape of a message dispatched from the Redux DevTools extension (id, payload, source, state, type).
#endregion

#region Design
// Generic over the payload and constructed only by derived request types (protected constructor). Settable
// properties so it binds from JSON.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

public class DispatchRequest<TPayload>
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
