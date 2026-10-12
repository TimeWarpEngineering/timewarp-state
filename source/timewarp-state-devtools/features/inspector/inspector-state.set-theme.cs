#region Purpose
// Sets the DevTools colour theme and applies it on the document element.
#endregion

#region Design
// The choice is InspectorState so it survives re-render. The document attribute is applied here,
// not in the component, so the click handler only dispatches. System removes data-theme and lets
// prefers-color-scheme win. A missing circuit (prerender) leaves the attribute alone; CSS already
// paints the system theme before script runs.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.Theme"/> and <c>data-theme</c> on <c>html</c>.</summary>
  public static class SetThemeActionSet
  {
    /// <summary>The set-theme action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string theme)
      {
        Theme = theme ?? "";
      }

      /// <summary><c>system</c>, <c>dark</c>, or <c>light</c>.</summary>
      public string Theme { get; }
    }

    /// <summary>Writes the theme and calls <c>timeWarpDevToolsTheme.apply</c>.</summary>
    public sealed class Handler(IStore store, IJSRuntime jsRuntime) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if
        (
          action.Theme != ThemeSystem
          && action.Theme != ThemeDark
          && action.Theme != ThemeLight
        )
        {
          return;
        }

        InspectorState.Theme = action.Theme;
        try
        {
          await jsRuntime.InvokeVoidAsync("timeWarpDevToolsTheme.apply", action.Theme);
        }
        catch (Exception exception) when
        (
          exception is JSException
            or JSDisconnectedException
            or InvalidOperationException
            or ObjectDisposedException
        )
        {
          // The circuit is not connected yet. The next successful toggle applies the attribute.
        }
      }
    }
  }
}
