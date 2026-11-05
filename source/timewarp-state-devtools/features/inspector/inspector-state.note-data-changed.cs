#region Purpose
// Bumps InspectorState.Revision after InspectorLog or InspectorManifest changes.
#endregion

#region Design
// The circuit dispatches this. It does not read or write InspectorLog or InspectorManifest, so it cannot append a
// span or store a manifest.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

partial class InspectorState
{
  /// <summary>Notifies subscribers that <see cref="InspectorLog"/> or <see cref="InspectorManifest"/> changed.</summary>
  public static class NoteDataChangedActionSet
  {
    /// <summary>The note-data-changed action.</summary>
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
