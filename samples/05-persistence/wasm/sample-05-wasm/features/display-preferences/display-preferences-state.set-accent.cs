#region Purpose
// Sets the accent. The persistence behavior writes local storage after this action.
#endregion

namespace Sample05Wasm.Features.DisplayPreferences;

partial class DisplayPreferencesState
{
  public static class SetAccentActionSet
  {
    public sealed class Action : IAction
    {
      public AccentKind Accent { get; }

      public Action(AccentKind accent)
      {
        Accent = accent;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private DisplayPreferencesState DisplayPreferencesState => Store.GetState<DisplayPreferencesState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        DisplayPreferencesState.Accent = action.Accent;
        return ValueTask.CompletedTask;
      }
    }
  }
}
