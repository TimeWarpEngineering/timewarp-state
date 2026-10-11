#region Purpose
// DevTools UI state: filter, selection, pause, and a revision the circuit re-renders on.
#endregion

#region Design
// The span list is InspectorLog, not a property here. A list on this state would be cloned by the
// DevTools compilation only, which is correct, but the HTTP ingest scope cannot see a scoped store.
// Revision bumps when the mailbox changes so subscribers re-render and read Snapshot. Filter,
// selection, and pause are ordinary actions. They are InspectorState, so InspectorCapture drops them
// if they are ever exported.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// UI state for the action-log page.
/// </summary>
public sealed partial class InspectorState : State<InspectorState>
{
  /// <summary>Increments when the displayed log should be read again.</summary>
  public int Revision { get; private set; }

  /// <summary>Case-insensitive substring applied to the log in the page.</summary>
  public string Filter { get; private set; } = "";

  /// <summary>Selected <see cref="ActionLogEntry.Sequence"/>, or 0 when nothing is selected.</summary>
  public long SelectedSequence { get; private set; }

  /// <summary>True when ingest is parking spans instead of showing them.</summary>
  public bool Paused { get; private set; }

  /// <inheritdoc />
  public override void Initialize()
  {
    Revision = 0;
    Filter = "";
    SelectedSequence = 0;
    Paused = false;
  }
}
