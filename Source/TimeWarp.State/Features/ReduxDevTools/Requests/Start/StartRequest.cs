namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Request received from Redux Dev Tools when one presses the Start Button.
/// </summary>
internal class StartRequest : DispatchRequest<StartRequest.PayloadClass>, IRequest, IReduxRequest
{
  internal class PayloadClass;
  public StartRequest(int id, PayloadClass payload, string source, string state, string type)
    : base(id, payload, source, state, type) {}
}
