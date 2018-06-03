namespace BlazorState
{
  using MediatR;

  /// <summary>
  /// Could be used if you don't use the Clone behavior.
  /// Not sure why you wouldn't just use the behavior.
  /// TODO: made this internal so won't be in public API docs
  /// Not sure this will be needed ever.
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TState"></typeparam>
  internal abstract class CloneStateRequestHandler<TRequest, TState> : RequestHandler<TRequest, TState>
    where TRequest : IRequest<TState>
    where TState : IState
  {
    protected CloneStateRequestHandler(
      IStore aStore) : base(aStore)
    {
      IState state = Store.GetState<TState>();
      Store = aStore;
      var newState = (TState)state.Clone();
      Store.SetState(newState);
    }
  }
}