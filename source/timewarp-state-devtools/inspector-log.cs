#region Purpose
// Process-wide action log shared by HTTP ingest and the Blazor circuit.
#endregion

#region Design
// IStore is scoped. An ingest request and a circuit would otherwise each hold an empty log.
// This mailbox is the one list both sides see. Add raises Changed outside the lock; the circuit
// dispatches NoteLogChanged, which re-renders from Snapshot. Pause parks spans in Pending so they
// stay out of the displayed list until Resume flushes them. UI actions never call Add.
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
        Entries.Add(stored);
        Trim(Entries);
        changed = Changed;
      }
    }

    changed?.Invoke();
  }

  /// <summary>
  /// Sets the pause flag. Does not move spans; <see cref="FlushPending"/> does that.
  /// </summary>
  public void SetPaused(bool paused)
  {
    lock (Sync)
    {
      Paused = paused;
    }
  }

  /// <summary>
  /// Moves the pause buffer onto the visible list and raises <see cref="Changed"/> when anything moved.
  /// </summary>
  public void FlushPending()
  {
    Action? changed = null;
    lock (Sync)
    {
      if (Pending.Count == 0)
      {
        return;
      }

      Entries.AddRange(Pending);
      Pending.Clear();
      Trim(Entries);
      changed = Changed;
    }

    changed?.Invoke();
  }

  /// <summary>
  /// Drops visible spans and the pause buffer.
  /// </summary>
  /// <param name="raiseChanged">When true and the visible list was non-empty, raises <see cref="Changed"/>.</param>
  public void Clear(bool raiseChanged)
  {
    Action? changed = null;
    lock (Sync)
    {
      bool hadEntries = Entries.Count > 0;
      Entries.Clear();
      Pending.Clear();
      if (raiseChanged && hadEntries)
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
      SnapshotTruncated = entry.SnapshotTruncated
    };
  }

  private static void Trim(List<ActionLogEntry> entries)
  {
    int overflow = entries.Count - MaxEntries;
    if (overflow > 0)
    {
      entries.RemoveRange(0, overflow);
    }
  }
}
