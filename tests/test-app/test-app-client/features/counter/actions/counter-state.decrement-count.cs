#region Purpose
// [Reducer] sample: subtracts Amount from CounterState.Count with no hand-written Handler.
#endregion

#region Design
// Task 144, Option 3b. The State generator emits DecrementCountActionSet.Handler, which calls Reduce. TimeWarp.Mediator
// dispatches the Action to that Handler because [Reducer] derives from GeneratedHandlerAttribute. The client
// integration tests send this action through the real ClientPipeline to prove the dispatch end to end.
// The action set is partial so the generated Handler can join it (TWS0014 otherwise).
#endregion

namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static partial class DecrementCountActionSet
  {
    [Reducer]
    public sealed class Action(int amount) : IAction
    {
      public int Amount { get; } = amount;
    }

    private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
  }
}
