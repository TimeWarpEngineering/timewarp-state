namespace Test.App.Client.Features.Blue;

public partial class BlueState
{
  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; init; }
    }

    internal sealed class Handler
    (
      IStore store
    ) : StateActionHandler<Action>(store)
    {

      BlueState BlueState => Store.GetState<BlueState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        BlueState.Count += action.Amount;
        return default;
      }
    }
  }
}
