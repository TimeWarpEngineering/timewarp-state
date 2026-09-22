#region Purpose
// Accent and density preferences stored in local storage.
#endregion

#region Design
// Local storage fits settings that should survive a new tab and a browser restart.
// compact defaults in the JsonConstructor so a stored payload from before that member existed
// still deserializes. That is ordinary System.Text.Json, not a migration framework.
// An accent string the enum converter cannot read throws JsonException from PersistenceService.
// The library does not rewrite or version the payload.
#endregion

namespace Sample05Wasm.Features.DisplayPreferences;

/// <summary>
/// Display choices kept for this origin.
/// </summary>
[PersistentState(PersistentStateMethod.LocalStorage)]
public sealed partial class DisplayPreferencesState : State<DisplayPreferencesState>
{
  public AccentKind Accent { get; private set; }

  public bool Compact { get; private set; }

  public DisplayPreferencesState() { }

  [JsonConstructor]
  public DisplayPreferencesState(Guid guid, AccentKind accent = AccentKind.Slate, bool compact = false)
  {
    Guid = guid;
    Accent = accent;
    Compact = compact;
  }

  public override void Initialize()
  {
    Accent = AccentKind.Slate;
    Compact = false;
  }

  public enum AccentKind
  {
    Slate,
    Sea,
    Ember
  }
}
