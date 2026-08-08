namespace Test.App.Client.Features.Blue;

public partial class BlueState
{
  public static class IncrementCountActionSet
  {
    internal sealed class Action : IAction
    {
      public int Amount { get; init; }
    }
    
    internal sealed class Handler
    (
      IStore store
    ) : ActionHandler<Action>(store)
    {

      BlueState BlueState => Store.GetState<BlueState>();

      public override ValueTask<Unit> Handle(Action action, CancellationToken cancellationToken)
      {
        BlueState.Count += action.Amount;
        return new ValueTask<Unit>(Unit.Value);
      }
    }
  }
}
