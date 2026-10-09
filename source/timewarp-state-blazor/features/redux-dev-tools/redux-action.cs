#region Purpose
// Wraps a request as a Redux action (type name plus payload) for sending to Redux DevTools.
#endregion

#region Design
// Internal. Type is the request's FullName, so DevTools shows fully qualified action names.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

internal class ReduxAction
{
  public ReduxAction(object request)
  {
    ArgumentNullException.ThrowIfNull(request);

    Type = request.GetType().FullName ?? throw new InvalidOperationException();
    Payload = request;
  }

  public object Payload { get; set; }
  public string Type { get; set; }
}
