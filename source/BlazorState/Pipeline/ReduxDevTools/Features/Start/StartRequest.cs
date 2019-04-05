namespace BlazorState.Pipeline.ReduxDevTools.Features.Start
{
  using MediatR;

  /// <summary>
  /// Request received from Redux Dev Tools when one presses the Start Button.
  /// </summary>
  internal class StartRequest : DispatchRequest<Payload>, IRequest, IReduxRequest { }

  internal class Payload { }
}