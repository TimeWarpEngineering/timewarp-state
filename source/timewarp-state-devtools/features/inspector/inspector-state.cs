#region Purpose
// DevTools UI state: filter, selection, pause, and a revision the circuit re-renders on.
#endregion

#region Design
// The span list is InspectorLog, not a property here. A list on this state would be cloned by the
// DevTools compilation only, which is correct, but the HTTP ingest scope cannot see a scoped store.
// Revision bumps when the mailbox changes so subscribers re-render and read Snapshot. Filter,
// selection, pause, tabs, theme, and node expansion are ordinary actions. They are InspectorState,
// so InspectorCapture drops them if they are ever exported.
// ExpandedNodes records an explicit open or closed choice. A path with no entry is open only at the
// root (no '.' and no '['), so new rows do not collapse a tree the user has already opened.
// Diff branches use their own keys (DiffKey) and default open, so every changed path is visible;
// the tree and the diff no longer share a choice.
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

  /// <summary>Detail tab: <c>action</c>, <c>state</c>, or <c>diff</c>.</summary>
  public string DetailTab { get; private set; } = DetailTabState;

  /// <summary>State panel mode: <c>tree</c> or <c>raw</c>.</summary>
  public string JsonView { get; private set; } = JsonViewTree;

  /// <summary>Theme override: <c>system</c>, <c>dark</c>, or <c>light</c>.</summary>
  public string Theme { get; private set; } = ThemeSystem;

  /// <summary>Explicit expand or collapse choice keyed by <see cref="StateTree.Node.Path"/>.</summary>
  public ImmutableDictionary<string, bool> ExpandedNodes { get; private set; } =
    ImmutableDictionary<string, bool>.Empty;

  /// <summary>Detail tab that lists the selected span.</summary>
  public const string DetailTabAction = "action";

  /// <summary>Detail tab that shows the folded state tree.</summary>
  public const string DetailTabState = "state";

  /// <summary>Detail tab that shows the structural diff.</summary>
  public const string DetailTabDiff = "diff";

  /// <summary>Collapsible JSON tree.</summary>
  public const string JsonViewTree = "tree";

  /// <summary>Raw snapshot text.</summary>
  public const string JsonViewRaw = "raw";

  /// <summary>Follow <c>prefers-color-scheme</c>.</summary>
  public const string ThemeSystem = "system";

  /// <summary>Force the dark theme.</summary>
  public const string ThemeDark = "dark";

  /// <summary>Force the light theme.</summary>
  public const string ThemeLight = "light";

  /// <summary>
  /// True when <paramref name="path"/> is expanded. Roots start open; every other path starts closed
  /// until <see cref="ExpandedNodes"/> records a choice.
  /// </summary>
  public bool IsNodeExpanded(string path)
  {
    if (ExpandedNodes.TryGetValue(path, out bool expanded))
    {
      return expanded;
    }

    return path.IndexOf('.', StringComparison.Ordinal) < 0
      && path.IndexOf('[', StringComparison.Ordinal) < 0;
  }

  /// <summary>Key in <see cref="ExpandedNodes"/> for a diff node, distinct from the tree's key for the same path.</summary>
  public static string DiffKey(string path) => "diff:" + path;

  /// <summary>
  /// True when the diff node at <paramref name="path"/> is expanded. Diff branches start open until
  /// <see cref="ExpandedNodes"/> records a choice under <see cref="DiffKey"/>.
  /// </summary>
  public bool IsDiffNodeExpanded(string path) =>
    !ExpandedNodes.TryGetValue(DiffKey(path), out bool expanded) || expanded;

  /// <inheritdoc />
  public override void Initialize()
  {
    Revision = 0;
    Filter = "";
    SelectedSequence = 0;
    Paused = false;
    DetailTab = DetailTabState;
    JsonView = JsonViewTree;
    Theme = ThemeSystem;
    ExpandedNodes = ImmutableDictionary<string, bool>.Empty;
  }
}
