#region Purpose
// Action that adds Amount to CounterState.Count; the app's main sample action.
#endregion

#region Design
// Nested ActionSet inside CounterState. The Counter component, the pre/post pipeline notification handlers and the
// JavaScript dispatch allow-list all key off this action type.
#endregion

namespace Test.App.Client.Features.Counter;

public partial class CounterState
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
      private CounterState CounterState => Store.GetState<CounterState>();

      public override ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        CounterState.Count += action.Amount;
        return default;
      }
    }
  }
}
