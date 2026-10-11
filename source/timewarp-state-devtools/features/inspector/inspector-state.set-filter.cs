#region Purpose
// Sets the action-log filter text.
#endregion

#region Design
// Page-local presentation. Does not touch InspectorLog.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Sets <see cref="InspectorState.Filter"/>.</summary>
  public static class SetFilterActionSet
  {
    /// <summary>The set-filter action.</summary>
    public sealed class Action : IAction
    {
      /// <summary>Creates the action.</summary>
      public Action(string filter)
      {
        Filter = filter ?? "";
      }

      /// <summary>Filter text.</summary>
      public string Filter { get; }
    }

    /// <summary>Writes the filter onto <see cref="InspectorState"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.Filter = action.Filter;
        return default;
      }
    }
  }
}
