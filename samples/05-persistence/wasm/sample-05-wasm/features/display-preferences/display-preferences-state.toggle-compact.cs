#region Purpose
// Flips compact density. The persistence behavior writes local storage after this action.
#endregion

namespace Sample05Wasm.Features.DisplayPreferences;

partial class DisplayPreferencesState
{
  public static class ToggleCompactActionSet
  {
    public sealed class Action : IAction;

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private DisplayPreferencesState DisplayPreferencesState => Store.GetState<DisplayPreferencesState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        DisplayPreferencesState.Compact = !DisplayPreferencesState.Compact;
        return ValueTask.CompletedTask;
      }
    }
  }
}
