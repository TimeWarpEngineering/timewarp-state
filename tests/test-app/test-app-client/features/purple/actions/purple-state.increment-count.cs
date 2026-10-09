#region Purpose
// Action that adds Amount to PurpleState.Count.
#endregion

#region Design
// Nested ActionSet inside PurpleState so the handler can set the private setter; used by the persistence test pages.
#endregion

namespace Test.App.Client.Features.Purple;

public partial class PurpleState
{
  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction
    {
      public int Amount { get; init; }
    }

    internal sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      PurpleState PurpleState => Store.GetState<PurpleState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        PurpleState.Count += action.Amount;
        return default;
      }
    }
  }
}
