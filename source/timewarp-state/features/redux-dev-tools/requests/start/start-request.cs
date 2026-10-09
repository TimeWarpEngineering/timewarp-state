#region Purpose
// Request sent by Redux DevTools when the Start button is pressed.
#endregion

#region Design
// DispatchRequest subtype with an empty nested PayloadClass. Marked IReduxRequest like the other DevTools requests.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Request received from Redux Dev Tools when one presses the Start Button.
/// </summary>
public class StartRequest : DispatchRequest<StartRequest.PayloadClass>, IRequest, IReduxRequest
{
  public class PayloadClass;
  public StartRequest(int id, PayloadClass payload, string source, string state, string type)
    : base(id, payload, source, state, type) {}
}
