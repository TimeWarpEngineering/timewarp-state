#region Purpose
// Pauses or resumes capture into the displayed log.
#endregion

#region Design
// The pause flag is on both InspectorState (the circuit) and InspectorLog (ingest). Resume flushes
// the buffer onto the visible list. The handler bumps Revision so the circuit re-renders even when
// FlushPending raises Changed on this same turn.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Toggles <see cref="InspectorState.Paused"/>.</summary>
  public static class TogglePauseActionSet
  {
    /// <summary>The toggle-pause action.</summary>
    public sealed class Action : IAction;

    /// <summary>Flips pause on the state and the mailbox.</summary>
    public sealed class Handler(IStore store, InspectorLog inspectorLog) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        bool paused = !InspectorState.Paused;
        InspectorState.Paused = paused;
        inspectorLog.SetPaused(paused);
        if (!paused)
        {
          inspectorLog.FlushPending();
        }

        InspectorState.Revision++;
        return default;
      }
    }
  }
}
