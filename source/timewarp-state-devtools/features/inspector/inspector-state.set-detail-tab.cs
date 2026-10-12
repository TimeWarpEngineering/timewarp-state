#region Purpose
// Selects the Action, State, or Diff tab.
#endregion

#region Design
// Page-local presentation. Does not touch InspectorLog. Unknown values are ignored so a bad dispatch
// cannot blank the pane.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.DetailTab"/>.</summary>
  public static class SetDetailTabActionSet
  {
    /// <summary>The set-detail-tab action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string tab)
      {
        Tab = tab ?? "";
      }

      /// <summary><c>action</c>, <c>state</c>, or <c>diff</c>.</summary>
      public string Tab { get; }
    }

    /// <summary>Writes the tab onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        if
        (
          action.Tab == DetailTabAction
          || action.Tab == DetailTabState
          || action.Tab == DetailTabDiff
        )
        {
          InspectorState.DetailTab = action.Tab;
        }

        return default;
      }
    }
  }
}
