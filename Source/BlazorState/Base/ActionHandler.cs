namespace BlazorState
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  public abstract class ActionHandler<TAction> : IRequestHandler<TAction>
  where TAction : IAction
  {
    public ActionHandler(IStore aStore)
    {
      Store = aStore;
    }

    protected IStore Store { get; set; }

    public abstract Task<Unit> Handle(TAction aAction, CancellationToken aCancellationToken);
  }
}