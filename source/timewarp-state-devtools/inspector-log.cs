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
// LatestByStateType tracks the newest snapshot per state type ever seen since the last Clear, so the
// current fold is one slot per state. FoldThrough walks entries only when a historical sequence is asked for.
// Trim does not touch the index: an idle state whose only snapshot was evicted from the display cap
// still appears in Current state. Only Clear resets it.
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
  private readonly Dictionary<string, FoldedState> LatestByStateType = new(StringComparer.Ordinal);
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
  /// Latest snapshot per state type across the visible list. One slot per state, not a walk of every entry.
  /// </summary>
  public IReadOnlyList<FoldedState> FoldCurrent()
  {
    lock (Sync)
    {
      return Order(LatestByStateType.Values);
    }
  }

  /// <summary>
  /// Latest snapshot per state type among visible entries with <see cref="ActionLogEntry.Sequence"/>
  /// less than or equal to <paramref name="sequence"/>.
  /// </summary>
  public IReadOnlyList<FoldedState> FoldThrough(long sequence)
  {
    lock (Sync)
    {
      Dictionary<string, FoldedState> folded = new(StringComparer.Ordinal);
      foreach (ActionLogEntry entry in Entries)
      {
        if (entry.Sequence > sequence)
        {
          break;
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
  /// Latest snapshot of <paramref name="stateTypeName"/> with sequence strictly less than <paramref name="sequence"/>.
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
      foreach (ActionLogEntry entry in Entries)
      {
        if (entry.Sequence >= sequence)
        {
          break;
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
          Note(LatestByStateType, entry);
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
      LatestByStateType.Clear();
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
      Attributes = entry.Attributes
    };
  }

  private void AppendVisible(ActionLogEntry stored)
  {
    Entries.Add(stored);
    Note(LatestByStateType, stored);
    Trim(Entries);
  }

  private static void Note(Dictionary<string, FoldedState> folded, ActionLogEntry entry)
  {
    if (entry.StateTypeName.Length == 0 || string.IsNullOrEmpty(entry.SnapshotJson))
    {
      return;
    }

    folded[entry.StateTypeName] = ToFolded(entry);
  }

  private static FoldedState ToFolded(ActionLogEntry entry) =>
    new()
    {
      StateTypeName = entry.StateTypeName,
      Sequence = entry.Sequence,
      Json = entry.SnapshotJson ?? "",
      Truncated = entry.SnapshotTruncated
    };

  private static FoldedState[] Order(IEnumerable<FoldedState> folded) =>
    folded.OrderBy(state => state.StateTypeName, StringComparer.Ordinal).ToArray();

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
