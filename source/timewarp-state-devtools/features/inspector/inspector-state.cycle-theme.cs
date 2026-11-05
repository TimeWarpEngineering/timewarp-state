#region Purpose
// Advances the theme override: system, dark, light, then system again.
#endregion

#region Design
// The header button dispatches this with no argument, so the order lives here and not in the page.
// Applying the attribute is shared with SetTheme through ApplyThemeAsync.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Moves <see cref="InspectorState.Theme"/> to the next choice.</summary>
  public static class CycleThemeActionSet
  {
    /// <summary>The cycle-theme action.</summary>
    public sealed class Action : IAction;

    /// <summary>Writes the next theme and calls <c>timeWarpDevToolsTheme.apply</c>.</summary>
    public sealed class Handler(IStore store, IJSRuntime jsRuntime) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        string next = NextTheme(InspectorState.Theme);
        InspectorState.Theme = next;
        await ApplyThemeAsync(jsRuntime, next);
      }
    }
  }

  /// <summary>Theme after <paramref name="theme"/> in the system, dark, light cycle.</summary>
  public static string NextTheme(string theme) =>
    theme switch
    {
      ThemeDark => ThemeLight,
      ThemeLight => ThemeSystem,
      _ => ThemeDark
    };
}
