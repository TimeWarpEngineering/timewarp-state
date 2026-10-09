#region Purpose
// Advances Beat. A card that registers only Count does not render for this action.
#endregion

#region Design
// Nested ActionSet with a parameterless Action. The handler increments ActivityState.Beat.
#endregion

namespace Sample06Wasm.Features.Activity;

partial class ActivityState
{
  public static class TickActionSet
  {
    public sealed class Action : IAction;

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private ActivityState ActivityState => Store.GetState<ActivityState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        ActivityState.Beat++;
        return ValueTask.CompletedTask;
      }
    }
  }
}
