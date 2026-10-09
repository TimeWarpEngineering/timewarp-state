#region Purpose
// Request sent by Redux DevTools when the Commit button is pressed.
#endregion

#region Design
// DispatchRequest subtype with a nested PayloadClass matching the extension's JSON. Marked IReduxRequest so DevTools
// requests can be identified.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

public class CommitRequest : DispatchRequest<CommitRequest.PayloadClass>, IRequest, IReduxRequest
{
  public class PayloadClass
  {
    public PayloadClass(string type) 
    {
      Type = type;
    }
    public string Type { get; }
  }

  public CommitRequest(int id, PayloadClass payload, string source, string state, string type) : base(id, payload, source, state, type) {}
}
