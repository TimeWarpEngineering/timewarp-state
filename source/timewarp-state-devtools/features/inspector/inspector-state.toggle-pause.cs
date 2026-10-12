#region Purpose
// Pauses or resumes capture into the displayed log.
#endregion

#region Design
// InspectorLog owns the pause flag; it is process-wide, so the toggle reads it rather than this
// circuit's copy, which another tab may have left stale. InspectorState.Paused mirrors it. Resume
// moves the buffer onto the visible list atomically inside SetPaused(false). The handler bumps Revision
// so the circuit re-renders even when SetPaused raises Changed on this same turn.
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
        bool paused = !inspectorLog.IsPaused;
        InspectorState.Paused = paused;
        inspectorLog.SetPaused(paused);
        InspectorState.Revision++;
        return default;
      }
    }
  }
}
