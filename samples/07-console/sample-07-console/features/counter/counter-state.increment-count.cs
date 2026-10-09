#region Purpose
// IncrementCount action for the console sample: adds Amount to Count.
#endregion

#region Design
// Nested ActionSet so the handler can set the private Count. The handler returns a completed ValueTask.
#endregion

namespace Sample07Console.Features.Counter;

partial class CounterState
{
  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; }

      public Action(int amount) => Amount = amount;
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private CounterState CounterState => Store.GetState<CounterState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        CounterState.Count += action.Amount;
        return default;
      }
    }
  }
}
