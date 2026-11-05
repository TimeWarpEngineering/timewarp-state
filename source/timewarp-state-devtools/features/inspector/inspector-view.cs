#region Purpose
// What the Inspector page shows for one render: rows, selection, store, fold, and diff.
#endregion

#region Design
// A read-only query over InspectorLog and InspectorState, built once per render so the page and
// its panels read one consistent snapshot. Nothing here dispatches or writes. The fold follows the
// selected row when that row belongs to the shown store, otherwise the store's latest fold.
// SelectedDiff pairs the selected row with its diff, so a panel cannot see one without the other.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// Render snapshot of the Inspector page.
/// </summary>
public sealed class InspectorView
{
  /// <summary>Log rows that pass the filter.</summary>
  public required IReadOnlyList<ActionLogEntry> Entries { get; init; }

  /// <summary>Selected row, or null when nothing is selected or the row is gone.</summary>
  public ActionLogEntry? Selected { get; init; }

  /// <summary>Selected row and its diff against the previous snapshot, or null with no selection.</summary>
  public ActionDiff? SelectedDiff { get; init; }

  /// <summary>Stores seen in the log, newest first.</summary>
  public required IReadOnlyList<StoreSummary> Stores { get; init; }

  /// <summary>Store the State panel folds.</summary>
  public required string StoreId { get; init; }

  /// <summary>Every state in the fold, before the filter.</summary>
  public required IReadOnlyList<FoldedState> Folded { get; init; }

  /// <summary>States in the fold that pass the filter.</summary>
  public required IReadOnlyList<FoldedState> States { get; init; }

  /// <summary>True when the fold stops at the selected row.</summary>
  public bool StateAfterAction { get; init; }

  /// <summary>A selected row with its structural diff.</summary>
  public sealed record ActionDiff(ActionLogEntry Entry, StateDiff.Result Result);

  /// <summary>Reads <paramref name="log"/> through the choices on <paramref name="state"/>.</summary>
  public static InspectorView Create(InspectorLog log, InspectorState state)
  {
    ArgumentNullException.ThrowIfNull(log);
    ArgumentNullException.ThrowIfNull(state);

    string filter = state.Filter.Trim();
    ActionLogEntry? selected = SelectedEntry(log, state.SelectedSequence);
    IReadOnlyList<StoreSummary> stores = log.ListStores();
    string storeId = StoreSelection.Effective(stores, state.SelectedStoreId);
    bool stateAfterAction = selected is not null
      && string.Equals(selected.StoreId, storeId, StringComparison.Ordinal);
    IReadOnlyList<FoldedState> folded = selected is not null && stateAfterAction
      ? log.FoldThrough(selected.Sequence)
      : log.FoldStore(storeId);

    return new InspectorView
    {
      Entries = FilterEntries(log.Snapshot(), filter),
      Selected = selected,
      SelectedDiff = selected is null ? null : new ActionDiff(selected, DiffFor(log, selected)),
      Stores = stores,
      StoreId = storeId,
      Folded = folded,
      States = FilterStates(folded, stores, storeId, filter),
      StateAfterAction = stateAfterAction
    };
  }

  /// <summary>
  /// Rows whose activity, action, state, or store badge contains <paramref name="filter"/>
  /// (case-insensitive). An empty filter keeps every row.
  /// </summary>
  public static IReadOnlyList<ActionLogEntry> FilterEntries(IReadOnlyList<ActionLogEntry> entries, string filter)
  {
    ArgumentNullException.ThrowIfNull(entries);
    if (string.IsNullOrEmpty(filter))
    {
      return entries;
    }

    return entries
      .Where(entry =>
        entry.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || entry.ActionName.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || entry.StateTypeName.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || StoreBadge.Matches(entry.StoreId, entry.Render, filter, entry.Runtime))
      .ToArray();
  }

  /// <summary>
  /// States whose type name contains <paramref name="filter"/>. When the filter matches the shown
  /// store's badge, every state is kept.
  /// </summary>
  public static IReadOnlyList<FoldedState> FilterStates
  (
    IReadOnlyList<FoldedState> folded,
    IReadOnlyList<StoreSummary> stores,
    string storeId,
    string filter
  )
  {
    ArgumentNullException.ThrowIfNull(folded);
    if (string.IsNullOrEmpty(filter) || StoreBadge.Matches(storeId, StoreSelection.RenderOf(stores, storeId), filter))
    {
      return folded;
    }

    return folded
      .Where(state => state.StateTypeName.Contains(filter, StringComparison.OrdinalIgnoreCase))
      .ToArray();
  }

  private static ActionLogEntry? SelectedEntry(InspectorLog log, long sequence)
  {
    if (sequence == 0)
    {
      return null;
    }

    return log.TryGetEntry(sequence, out ActionLogEntry? entry) ? entry : null;
  }

  private static StateDiff.Result DiffFor(InspectorLog log, ActionLogEntry entry)
  {
    string rootName = entry.StateTypeName.Length == 0 ? entry.Name : entry.StateTypeName;
    log.TryGetPreviousSnapshot(entry.StateTypeName, entry.Sequence, out FoldedState? previous);
    return StateDiff.Compare
    (
      previous?.Json,
      previous?.Truncated ?? false,
      entry.SnapshotJson,
      entry.SnapshotTruncated,
      rootName.Length == 0 ? "state" : rootName
    );
  }
}
