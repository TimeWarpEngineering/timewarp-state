#region Purpose
// Action that adds Amount to BlueState.Count.
#endregion

#region Design
// Nested ActionSet (Action plus Handler) inside the state, the convention the TimeWarp.State analyzers enforce, so the
// handler can set the private setter. Used by the persistence and subscription tests.
#endregion

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
