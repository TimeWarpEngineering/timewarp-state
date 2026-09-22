#region Purpose
// Per-scope last-JSON cache so diffs are string compares, not property walks.
#endregion

#region Design
// Scoped to match IStore (one Blazor Server circuit). Keys are Type.FullName, never
// AssemblyQualifiedName. First JSON for a state type is the full snapshot; later unequal
// JSON is a diff event; equal JSON emits nothing.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// Holds the last serialized snapshot per state type for the current scope.
/// </summary>
public sealed class StateSnapshotCache
{
  private readonly ConcurrentDictionary<string, string> LastJsonByStateTypeName = new();

  /// <summary>
  /// Records <paramref name="json"/> for <paramref name="stateTypeName"/> and returns how it compares
  /// to the previous value.
  /// </summary>
  public SnapshotChange Record(string stateTypeName, string json)
  {
    ArgumentException.ThrowIfNullOrEmpty(stateTypeName);
    ArgumentNullException.ThrowIfNull(json);

    if (!LastJsonByStateTypeName.TryGetValue(stateTypeName, out string? lastJson))
    {
      LastJsonByStateTypeName[stateTypeName] = json;
      return SnapshotChange.Initial;
    }

    if (string.Equals(lastJson, json, StringComparison.Ordinal))
    {
      return SnapshotChange.Unchanged;
    }

    LastJsonByStateTypeName[stateTypeName] = json;
    return SnapshotChange.Changed;
  }
}

/// <summary>
/// Result of comparing a new snapshot to the cached JSON for that state type.
/// </summary>
public enum SnapshotChange
{
  /// <summary>
  /// First snapshot for this state type in the scope.
  /// </summary>
  Initial = 0,

  /// <summary>
  /// JSON differs from the last recorded snapshot.
  /// </summary>
  Changed = 1,

  /// <summary>
  /// JSON is identical to the last recorded snapshot.
  /// </summary>
  Unchanged = 2
}
