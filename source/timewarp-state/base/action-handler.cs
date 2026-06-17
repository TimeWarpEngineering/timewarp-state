namespace TimeWarp.State;

public abstract class ActionHandler<TAction>
(
  IStore store
) : IRequestHandler<TAction> where TAction : IAction
{
  protected IStore Store { get; set; } = store;

  public abstract ValueTask<Unit> Handle(TAction action, CancellationToken cancellationToken);
}
