#region Purpose
// Sets the accent. The persistence behavior writes local storage after this action.
#endregion

#region Design
// [Reducer] (short) shape: the State generator emits SetAccentActionSet.Handler, which calls Reduce with the state
// from the store. The action set is partial so the generated Handler can join it (TWS0014 otherwise). DraftNoteState
// keeps the hand-written Handler (long) shape so the sample shows both.
#endregion

namespace Sample05Wasm.Features.DisplayPreferences;

partial class DisplayPreferencesState
{
  public static partial class SetAccentActionSet
  {
    [Reducer]
    public sealed class Action(AccentKind accent) : IAction
    {
      public AccentKind Accent { get; } = accent;
    }

    private static void Reduce(DisplayPreferencesState state, Action action) => state.Accent = action.Accent;
  }
}
