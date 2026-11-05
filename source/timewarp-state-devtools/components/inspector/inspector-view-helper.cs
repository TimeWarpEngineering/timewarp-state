#region Purpose
// Pure formatting for the Inspector page and its panels: labels, CSS classes, and tab lists.
#endregion

#region Design
// Static and stateless. Each method takes the values it formats, so the components read state and
// pass it in, and nothing here touches InspectorLog or dispatches.
#endregion

namespace TimeWarp.State.DevTools.Components.Inspector;

/// <summary>
/// Formatting helpers for the Inspector components.
/// </summary>
public static class InspectorViewHelper
{
  /// <summary>One tab button: the value it selects, its label, and its <c>data-qa</c>.</summary>
  public sealed record Tab(string Value, string Label, string Qa);

  /// <summary>Detail panel tabs, in display order.</summary>
  public static IReadOnlyList<Tab> DetailTabs { get; } =
  [
    new(InspectorState.DetailTabAction, "Action", "tab-action"),
    new(InspectorState.DetailTabState, "State", "tab-state"),
    new(InspectorState.DetailTabDiff, "Diff", "tab-diff")
  ];

  /// <summary>State panel format tabs, in display order.</summary>
  public static IReadOnlyList<Tab> JsonViewTabs { get; } =
  [
    new(InspectorState.JsonViewTree, "Tree", "view-tree"),
    new(InspectorState.JsonViewRaw, "Raw", "view-raw")
  ];

  /// <summary>Header label for <paramref name="theme"/>.</summary>
  public static string ThemeLabel(string theme) =>
    theme switch
    {
      InspectorState.ThemeDark => "Dark",
      InspectorState.ThemeLight => "Light",
      _ => "System"
    };

  /// <summary>Which side of a truncated diff was cut.</summary>
  public static string TruncatedHint(StateDiff.Result diff)
  {
    ArgumentNullException.ThrowIfNull(diff);
    return (diff.BeforeTruncated, diff.AfterTruncated) switch
    {
      (true, true) => "The previous and selected snapshots are truncated.",
      (true, false) => "The previous snapshot is truncated.",
      _ => "The selected snapshot is truncated."
    };
  }

  /// <summary>Store picker value for the current pin, or follow when the pin is not listed.</summary>
  public static string StoreSelectValue(IReadOnlyList<StoreSummary> stores, string? pinnedStoreId)
  {
    if (!StoreSelection.IsListed(stores, pinnedStoreId) || pinnedStoreId is null)
    {
      return InspectorState.StoreOptionFollow;
    }

    return pinnedStoreId.Length == 0 ? InspectorState.StoreOptionUntagged : pinnedStoreId;
  }

  /// <summary>Store picker option value for <paramref name="store"/>.</summary>
  public static string StoreOptionValue(StoreSummary store)
  {
    ArgumentNullException.ThrowIfNull(store);
    return store.StoreId.Length == 0 ? InspectorState.StoreOptionUntagged : store.StoreId;
  }

  /// <summary>Store picker option text: badge and id, the bare id, or <c>(untagged)</c>.</summary>
  public static string StoreOptionText(StoreSummary store)
  {
    ArgumentNullException.ThrowIfNull(store);
    if (store.StoreId.Length == 0)
    {
      return "(untagged)";
    }

    string label = StoreBadge.Label(store.StoreId, store.Render);
    if (label.Length == 0)
    {
      return store.StoreId;
    }

    return label + " " + store.StoreId;
  }

  /// <summary>Row classes: <c>row</c>, plus <c>selected</c> and <c>init</c> when they apply.</summary>
  public static string RowClass(ActionLogEntry entry, long selectedSequence)
  {
    ArgumentNullException.ThrowIfNull(entry);
    string css = "row";
    if (entry.Sequence == selectedSequence)
    {
      css += " selected";
    }

    if (string.Equals(entry.Kind, InspectorCapture.InitKind, StringComparison.Ordinal))
    {
      css += " init";
    }

    return css;
  }

  /// <summary><c>tab active</c> for the current tab, otherwise <c>tab</c>.</summary>
  public static string TabClass(string tab, string current) =>
    tab == current ? "tab active" : "tab";

  /// <summary>Store badge classes, prefixed with <paramref name="prefix"/> when given.</summary>
  public static string BadgeClass(string? prefix, string label)
  {
    ArgumentNullException.ThrowIfNull(label);
    string css = label.Length == 0 ? "store-badge" : "store-badge on";
    return string.IsNullOrEmpty(prefix) ? css : prefix + " " + css;
  }

  /// <summary>Row time cell: local time to the millisecond.</summary>
  public static string RowTime(DateTime startTimeUtc) =>
    startTimeUtc.ToLocalTime().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

  /// <summary>Duration in milliseconds with one decimal and a <c>ms</c> unit.</summary>
  public static string Duration(double milliseconds) =>
    milliseconds.ToString("0.0", CultureInfo.InvariantCulture) + " ms";
}
