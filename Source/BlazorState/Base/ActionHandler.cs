namespace BlazorState
{
  using MediatR;
  using System.Threading;
  using System.Threading.Tasks;

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