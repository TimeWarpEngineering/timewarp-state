namespace BlazorState
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  public abstract class RequestHandler<TRequest, TState> : IRequestHandler<TRequest, TState>
    where TRequest : IRequest<TState>
    where TState : IState
  {
    public RequestHandler(
      IStore aStore)
    {
      Store = aStore;
    }

    protected IState State => Store.GetState<TState>();
    protected IStore Store { get; set; }

    public abstract Task<TState> Handle(TRequest aRequest, CancellationToken aCancellationToken);
  }
}