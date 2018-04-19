namespace BlazorState.Handlers
{
  using BlazorState.State;
  using BlazorState.Store;
  using MediatR;

  /// <summary>
  /// Could be used if you don't use the Clone behavior.
  /// Not sure why you wouldn't just use the behavior.
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TState"></typeparam>
  public abstract class CloneStateRequestHandler<TRequest, TState> : RequestHandler<TRequest, TState>
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