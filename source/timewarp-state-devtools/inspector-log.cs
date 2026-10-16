#region Purpose
// Process-wide action log shared by HTTP ingest and the Blazor circuit.
#endregion

#region Design
// IStore is scoped. An ingest request and a circuit would otherwise each hold an empty log.
// This mailbox is the one list both sides see. Add raises Changed outside the lock; the circuit
// dispatches NoteLogChanged, which re-renders from Snapshot. SetPaused and Clear raise Changed too,
// so a second tab sees the shared pause flag and list. Pause parks spans in Pending so they
// stay out of the displayed list until SetPaused(false) moves them onto Entries inside the same lock,
// so no ingest can interleave and Entries stays in sequence order. UI actions never call Add.
// LatestByStoreAndState tracks the newest snapshot per store and state type since the last Clear.
// ListStores orders stores by their first span, newest first. Current state is StoreSelection.Default:
// the newest store that is not a prerender, so a prerender from enhanced navigation does not replace
// the live circuit's fold. A later span on an older store updates that store's fold only.
// FoldThrough walks that store's entries up to the asked sequence. Trim does not touch the index:
// an idle state whose only snapshot was evicted from the display cap still appears in its store's fold.
// Only Clear resets the index. WithSequence copies every ActionLogEntry field, including Runtime,
// because the stored row is a new instance.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// Displayed spans and the pause buffer.
/// </summary>
public sealed class InspectorLog
{
  /// <summary>Visible entries kept after the cap.</summary>
  public const int MaxEntries = 500;

  private readonly object Sync = new();
  private readonly List<ActionLogEntry> Entries = [];
  private readonly List<ActionLogEntry> Pending = [];
  private readonly Dictionary<string, FoldedState> LatestByStoreAndState = new(StringComparer.Ordinal);
  private long NextSequence = 1;
  private bool Paused;

  /// <summary>Raised after the visible list changes.</summary>
  public event Action? Changed;

  /// <summary>
  /// True when new spans wait in the pause buffer.
  /// </summary>
  public bool IsPaused
  {
    get
    {
      lock (Sync)
      {
        return Paused;
      }
    }
  }

  /// <summary>
  /// Copies the visible list.
  /// </summary>
  public IReadOnlyList<ActionLogEntry> Snapshot()
  {
    lock (Sync)
    {
      return Entries.ToArray();
    }
  }

  /// <summary>
  /// Stores that have a visible span or a folded snapshot, newest first.
  /// </summary>
  public IReadOnlyList<StoreSummary> ListStores()
  {
    lock (Sync)
    {
      return OrderStores(CollectStores());
    }
  }

  /// <summary>
  /// Latest snapshot per state type for the default store (<see cref="StoreSelection.Default"/>):
  /// the newest store that is not a prerender. One slot per state in that store.
  /// </summary>
  public IReadOnlyList<FoldedState> FoldCurrent()
  {
    lock (Sync)
    {
      StoreSummary? store = StoreSelection.Default(OrderStores(CollectStores()));
      return store is null ? [] : FoldStoreLocked(store.StoreId);
    }
  }

  /// <summary>
  /// Latest snapshot per state type for <paramref name="storeId"/>, including snapshots whose rows were trimmed.
  /// </summary>
  public IReadOnlyList<FoldedState> FoldStore(string storeId)
  {
    ArgumentNullException.ThrowIfNull(storeId);
    lock (Sync)
    {
      return FoldStoreLocked(storeId);
    }
  }

  /// <summary>
  /// Latest snapshot per state type for the store that owns <paramref name="sequence"/>,
  /// among that store's visible entries with <see cref="ActionLogEntry.Sequence"/> less than or equal to it.
  /// </summary>
  public IReadOnlyList<FoldedState> FoldThrough(long sequence)
  {
    lock (Sync)
    {
      string? storeId = StoreIdOf(sequence);
      Dictionary<string, FoldedState> folded = new(StringComparer.Ordinal);
      foreach (ActionLogEntry entry in Entries)
      {
        if (entry.Sequence > sequence)
        {
          break;
        }

        if (storeId is not null && !string.Equals(entry.StoreId, storeId, StringComparison.Ordinal))
        {
          continue;
        }

        Note(folded, entry);
      }

      return Order(folded.Values);
    }
  }

  /// <summary>
  /// Finds the visible entry with <paramref name="sequence"/>.
  /// </summary>
  public bool TryGetEntry(long sequence, out ActionLogEntry? entry)
  {
    lock (Sync)
    {
      foreach (ActionLogEntry candidate in Entries)
      {
        if (candidate.Sequence == sequence)
        {
          entry = candidate;
          return true;
        }
      }
    }

    entry = null;
    return false;
  }

  /// <summary>
  /// Latest snapshot of <paramref name="stateTypeName"/> in the same store as <paramref name="sequence"/>,
  /// with sequence strictly less than <paramref name="sequence"/>.
  /// </summary>
  public bool TryGetPreviousSnapshot(string stateTypeName, long sequence, out FoldedState? previous)
  {
    ArgumentNullException.ThrowIfNull(stateTypeName);
    previous = null;
    if (stateTypeName.Length == 0)
    {
      return false;
    }

    lock (Sync)
    {
      string? storeId = StoreIdOf(sequence);
      if (storeId is null)
      {
        return false;
      }

      foreach (ActionLogEntry entry in Entries)
      {
        if (entry.Sequence >= sequence)
        {
          break;
        }

        if (!string.Equals(entry.StoreId, storeId, StringComparison.Ordinal))
        {
          continue;
        }

        if (!string.Equals(entry.StateTypeName, stateTypeName, StringComparison.Ordinal))
        {
          continue;
        }

        if (string.IsNullOrEmpty(entry.SnapshotJson))
        {
          continue;
        }

        previous = ToFolded(entry);
      }
    }

    return previous is not null;
  }

  /// <summary>
  /// Appends <paramref name="entry"/> to the visible list, or to the pause buffer when paused.
  /// Assigns <see cref="ActionLogEntry.Sequence"/>.
  /// </summary>
  public void Add(ActionLogEntry entry)
  {
    ArgumentNullException.ThrowIfNull(entry);
    Action? changed = null;
    lock (Sync)
    {
      ActionLogEntry stored = WithSequence(entry);
      if (Paused)
      {
        Pending.Add(stored);
        Trim(Pending);
      }
      else
      {
        AppendVisible(stored);
        changed = Changed;
      }
    }

    changed?.Invoke();
  }

  /// <summary>
  /// Sets the pause flag and raises <see cref="Changed"/> when the flag changed, so every circuit
  /// refreshes its pause display. Resuming moves the pause buffer onto the visible list in the same
  /// lock acquisition, so no concurrent <see cref="Add"/> can interleave with the flush.
  /// </summary>
  public void SetPaused(bool paused)
  {
    Action? changed = null;
    lock (Sync)
    {
      if (Paused != paused)
      {
        Paused = paused;
        changed = Changed;
      }

      if (!paused && Pending.Count > 0)
      {
        foreach (ActionLogEntry entry in Pending)
        {
          Entries.Add(entry);
          Note(LatestByStoreAndState, entry);
        }

        Pending.Clear();
        Trim(Entries);
        changed = Changed;
      }
    }

    changed?.Invoke();
  }

  /// <summary>
  /// Drops visible spans and the pause buffer.
  /// </summary>
  /// <param name="raiseChanged">When true and anything was dropped, raises <see cref="Changed"/> so every circuit refreshes.</param>
  public void Clear(bool raiseChanged)
  {
    Action? changed = null;
    lock (Sync)
    {
      bool hadAny = Entries.Count > 0 || Pending.Count > 0;
      Entries.Clear();
      Pending.Clear();
      LatestByStoreAndState.Clear();
      if (raiseChanged && hadAny)
      {
        changed = Changed;
      }
    }

    changed?.Invoke();
  }

  private ActionLogEntry WithSequence(ActionLogEntry entry)
  {
    long sequence = NextSequence;
    NextSequence++;
    return new ActionLogEntry
    {
      Sequence = sequence,
      Name = entry.Name,
      ActionName = entry.ActionName,
      StateTypeName = entry.StateTypeName,
      Status = entry.Status,
      DurationMilliseconds = entry.DurationMilliseconds,
      StartTimeUtc = entry.StartTimeUtc,
      SpanId = entry.SpanId,
      SnapshotJson = entry.SnapshotJson,
      SnapshotKind = entry.SnapshotKind,
      SnapshotTruncated = entry.SnapshotTruncated,
      Kind = entry.Kind,
      StoreId = entry.StoreId,
      Render = entry.Render,
      Runtime = entry.Runtime,
      Attributes = entry.Attributes
    };
  }

  private void AppendVisible(ActionLogEntry stored)
  {
    Entries.Add(stored);
    Note(LatestByStoreAndState, stored);
    Trim(Entries);
  }

  private IReadOnlyList<FoldedState> FoldStoreLocked(string storeId)
  {
    List<FoldedState> folded = [];
    foreach (KeyValuePair<string, FoldedState> pair in LatestByStoreAndState)
    {
      if (string.Equals(StoreIdFromKey(pair.Key), storeId, StringComparison.Ordinal))
      {
        folded.Add(pair.Value);
      }
    }

    return Order(folded);
  }

  private Dictionary<string, StoreWork> CollectStores()
  {
    Dictionary<string, StoreWork> stores = new(StringComparer.Ordinal);
    foreach (ActionLogEntry entry in Entries)
    {
      Touch(stores, entry.StoreId, entry.Render, entry.Sequence);
    }

    foreach (KeyValuePair<string, FoldedState> pair in LatestByStoreAndState)
    {
      Touch(stores, pair.Value.StoreId, pair.Value.Render, pair.Value.Sequence);
    }

    return stores;
  }

  private static void Touch(Dictionary<string, StoreWork> stores, string storeId, string render, long sequence)
  {
    if (!stores.TryGetValue(storeId, out StoreWork? store))
    {
      stores[storeId] = new StoreWork(storeId, render, sequence);
      return;
    }

    if (sequence < store.FirstSequence)
    {
      store.FirstSequence = sequence;
    }

    if (sequence >= store.LatestSequence)
    {
      store.LatestSequence = sequence;
      if (render.Length > 0)
      {
        store.Render = render;
      }
    }
  }

  private static StoreSummary[] OrderStores(Dictionary<string, StoreWork> stores) =>
    stores.Values
      .OrderByDescending(store => store.FirstSequence)
      .ThenBy(store => store.StoreId, StringComparer.Ordinal)
      .Select(store => new StoreSummary
      {
        StoreId = store.StoreId,
        Render = store.Render,
        LatestSequence = store.LatestSequence
      })
      .ToArray();

  private string? StoreIdOf(long sequence)
  {
    foreach (ActionLogEntry entry in Entries)
    {
      if (entry.Sequence == sequence)
      {
        return entry.StoreId;
      }
    }

    return null;
  }

  private static string StoreIdFromKey(string key)
  {
    int split = key.IndexOf('\n');
    return split < 0 ? key : key[..split];
  }

  private static void Note(Dictionary<string, FoldedState> folded, ActionLogEntry entry)
  {
    if (entry.StateTypeName.Length == 0 || string.IsNullOrEmpty(entry.SnapshotJson))
    {
      return;
    }

    folded[entry.StoreId + "\n" + entry.StateTypeName] = ToFolded(entry);
  }

  private static FoldedState ToFolded(ActionLogEntry entry) =>
    new()
    {
      StateTypeName = entry.StateTypeName,
      Sequence = entry.Sequence,
      Json = entry.SnapshotJson ?? "",
      Truncated = entry.SnapshotTruncated,
      StoreId = entry.StoreId,
      Render = entry.Render
    };

  private static FoldedState[] Order(IEnumerable<FoldedState> folded) =>
    folded.OrderBy(state => state.StateTypeName, StringComparer.Ordinal).ToArray();

  private sealed class StoreWork(string storeId, string render, long sequence)
  {
    public string StoreId { get; } = storeId;

    public string Render { get; set; } = render;

    public long FirstSequence { get; set; } = sequence;

    public long LatestSequence { get; set; } = sequence;
  }

  private static bool Trim(List<ActionLogEntry> entries)
  {
    int overflow = entries.Count - MaxEntries;
    if (overflow > 0)
    {
      entries.RemoveRange(0, overflow);
      return true;
    }

    return false;
  }
}
