namespace BlazorState.Pipeline.ReduxDevTools;

/// <summary>
/// Request received from Redux Dev Tools when one presses the Start Button.
/// </summary>
internal class StartRequest : DispatchRequest<StartRequest.PayloadClass>, IRequest, IReduxRequest
{

  internal class PayloadClass { }
}
