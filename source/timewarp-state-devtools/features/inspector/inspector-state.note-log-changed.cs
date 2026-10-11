#region Purpose
// Bumps InspectorState.Revision after InspectorLog changes.
#endregion

#region Design
// The circuit dispatches this. It does not read or write InspectorLog, so it cannot append a span.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Notifies subscribers that <see cref="InspectorLog"/> changed.</summary>
  public static class NoteLogChangedActionSet
  {
    /// <summary>The note-log-changed action.</summary>
    public sealed class Action : IAction;

    /// <summary>Increments <see cref="InspectorState.Revision"/>.</summary>
    public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
    {
      private InspectorState InspectorState => Store.GetState<InspectorState>();

      /// <inheritdoc />
      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        InspectorState.Revision++;
        return default;
      }
    }
  }
}
