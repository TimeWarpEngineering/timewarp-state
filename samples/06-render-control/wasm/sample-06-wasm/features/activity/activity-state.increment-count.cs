#region Purpose
// Adds to Count. Cards that register Count render for this action.
#endregion

namespace Sample06Wasm.Features.Activity;

partial class ActivityState
{
  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; }

      public Action(int amount)
      {
        Amount = amount;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private ActivityState ActivityState => Store.GetState<ActivityState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        ActivityState.Count += action.Amount;
        return ValueTask.CompletedTask;
      }
    }
  }
}
