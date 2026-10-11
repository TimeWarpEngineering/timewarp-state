#region Purpose
// Clears the displayed log and the pause buffer.
#endregion

#region Design
// Clear on InspectorLog does not raise Changed; this handler bumps Revision so the circuit
// re-renders the empty snapshot. Selection is dropped with the rows.
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
        inspectorLog.Clear(raiseChanged: false);
        InspectorState.SelectedSequence = 0;
        InspectorState.Revision++;
        return default;
      }
    }
  }
}
