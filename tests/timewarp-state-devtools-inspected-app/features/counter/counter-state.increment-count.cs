#region Purpose
// IncrementCount action for the inspected-app fixture: adds Amount to Count.
#endregion

#region Design
// Nested ActionSet (Action + Handler) inside the partial CounterState so the handler can set the private Count. The mediator generator weaves this action through StateTransactionBehavior. The handler is synchronous and returns a completed ValueTask.
#endregion

namespace InspectedApp.Features.Counter;

partial class CounterState
{
  /// <summary>
  /// Action and handler that add to <see cref="CounterState.Count"/>.
  /// </summary>
  public static class IncrementCountActionSet
  {
    /// <summary>
    /// Request to add <see cref="Amount"/> to the count.
    /// </summary>
    public sealed class Action : IAction
    {
      /// <summary>
      /// Amount added to the count.
      /// </summary>
      public int Amount { get; }

      /// <summary>
      /// Creates the action.
      /// </summary>
      /// <param name="amount">Amount added to the count.</param>
      public Action(int amount)
      {
        Amount = amount;
      }
    }

    /// <summary>
    /// Adds <see cref="Action.Amount"/> to <see cref="CounterState.Count"/>.
    /// </summary>
    public sealed class Handler : StateActionHandler<Action>
    {
      /// <summary>
      /// Creates the handler.
      /// </summary>
      /// <param name="store">Store that holds <see cref="CounterState"/>.</param>
      public Handler(IStore store) : base(store) { }

      private CounterState CounterState => Store.GetState<CounterState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        CounterState.Count += action.Amount;
        return default;
      }
    }
  }
}
