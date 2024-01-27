namespace BlazorState;

public abstract class ActionHandler<TAction>
(
  IStore store
) : IRequestHandler<TAction> where TAction : IAction
{
  protected IStore Store { get; set; } = store;

  public abstract Task Handle(TAction aAction, CancellationToken aCancellationToken);
}
