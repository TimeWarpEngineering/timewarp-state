#region Purpose
// Flips compact density. The persistence behavior writes local storage after this action.
#endregion

#region Design
// [Reducer] (short) shape with a parameterless Action. The State generator emits ToggleCompactActionSet.Handler,
// which calls Reduce; the generated ToggleCompact() dispatch method is the same as for a hand-written Handler.
#endregion

namespace Sample05Wasm.Features.DisplayPreferences;

partial class DisplayPreferencesState
{
  public static partial class ToggleCompactActionSet
  {
    [Reducer]
    public sealed class Action : IAction;

    private static void Reduce(DisplayPreferencesState state, Action action) => state.Compact = !state.Compact;
  }
}
