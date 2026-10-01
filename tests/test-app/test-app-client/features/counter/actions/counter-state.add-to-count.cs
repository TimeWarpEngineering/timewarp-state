#region Purpose
// Cataloged counter action with an optional parameter; exercised by the action catalog integration tests.
#endregion

namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class AddToCountActionSet
  {
    [CatalogAction
    (
      Description = "Add an amount to the counter.",
      DisplayName = "Add to Count",
      Permissions = ["counter.write"],
      Visibility = ActionVisibility.Both
    )]
    public sealed class Action : IAction
    {
      public int Amount { get; }
      public int Multiplier { get; }

      public Action(int amount, int multiplier = 1)
      {
        Amount = amount;
        Multiplier = multiplier;
      }
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
        CounterState.Count += action.Amount * action.Multiplier;
        return default;
      }
    }
  }
}
