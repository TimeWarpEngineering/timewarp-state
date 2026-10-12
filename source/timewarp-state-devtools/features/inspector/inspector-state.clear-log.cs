#region Purpose
// Clears the displayed log and the pause buffer.
#endregion

#region Design
// Clear on InspectorLog raises Changed so other circuits refresh; this handler also bumps Revision so
// this circuit re-renders the empty snapshot. Selection and the store pin are dropped with the rows,
// so the State panel follows the default store again.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Clears the log.</summary>
  public static class ClearLogActionSet
  {
    /// <summary>The clear-log action.</summary>
    public sealed class Action : IAction;

    /// <summary>Empties <see cref="InspectorLog"/> and the selection.</summary>
    public sealed class Handler(IStore store, InspectorLog inspectorLog) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        inspectorLog.Clear(raiseChanged: true);
        InspectorState.SelectedSequence = 0;
        InspectorState.SelectedStoreId = null;
        InspectorState.Revision++;
        return default;
      }
    }
  }
}
