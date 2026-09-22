#region Purpose
// Tab-scoped draft text stored in session storage.
#endregion

#region Design
// Session storage fits unsaved text: refresh in this tab restores it; a new tab does not see it.
// The JsonConstructor parameter text defaults to empty so a payload that omits the member still binds.
// TimeWarp.State has no migration type. See the sample README for the FullName/Name key fallback.
#endregion

namespace Sample05Wasm.Features.DraftNote;

/// <summary>
/// Unsaved note text for this browser tab.
/// </summary>
[PersistentState(PersistentStateMethod.SessionStorage)]
public sealed partial class DraftNoteState : State<DraftNoteState>
{
  public string Text { get; private set; } = "";

  public DraftNoteState() { }

  [JsonConstructor]
  public DraftNoteState(Guid guid, string? text = "")
  {
    Guid = guid;
    Text = text ?? "";
  }

  public override void Initialize() => Text = "";
}
