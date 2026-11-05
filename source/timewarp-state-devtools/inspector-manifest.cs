#region Purpose
// Process-wide registry of the State manifests the inspected apps posted, one per assembly.
#endregion

#region Design
// A singleton like InspectorLog, so the ingest request and every circuit see the same list. The latest manifest
// for an assembly name replaces the earlier one, so a restarted app does not pile up duplicates. The count is
// capped because the input is untrusted: past MaxAssemblies a new assembly name is refused, while an assembly
// already known can still be replaced. Changed is raised outside the lock, and only when something changed. SetAll
// stores a whole ingest request under one lock and raises Changed once, so a request does not re-render N times.
// The manifests are read-only data: nothing here dispatches an action in the inspected app.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// The State manifests posted by inspected apps.
/// </summary>
public sealed class InspectorManifest
{
  /// <summary>Most distinct assemblies kept.</summary>
  public const int MaxAssemblies = 64;

  private readonly object Sync = new();
  private readonly Dictionary<string, StateManifest> ByAssembly = new(StringComparer.Ordinal);

  /// <summary>Raised after the stored manifests change.</summary>
  public event Action? Changed;

  /// <summary>
  /// Stores <paramref name="manifest"/>, replacing any manifest for the same assembly name.
  /// </summary>
  /// <param name="manifest">A parsed manifest.</param>
  /// <returns>False when the assembly is new and <see cref="MaxAssemblies"/> is reached.</returns>
  public bool Set(StateManifest manifest)
  {
    ArgumentNullException.ThrowIfNull(manifest);
    Action? changed;
    lock (Sync)
    {
      if (!ByAssembly.ContainsKey(manifest.AssemblyName) && ByAssembly.Count >= MaxAssemblies)
      {
        return false;
      }

      ByAssembly[manifest.AssemblyName] = manifest;
      changed = Changed;
    }

    changed?.Invoke();
    return true;
  }

  /// <summary>
  /// Stores each manifest in <paramref name="manifests"/> as <see cref="Set"/> does, and raises
  /// <see cref="Changed"/> at most once for the whole batch.
  /// </summary>
  /// <param name="manifests">Parsed manifests, applied in order.</param>
  /// <returns>How many were stored. A new assembly past <see cref="MaxAssemblies"/> is not.</returns>
  public int SetAll(IEnumerable<StateManifest> manifests)
  {
    ArgumentNullException.ThrowIfNull(manifests);
    int stored = 0;
    Action? changed;
    lock (Sync)
    {
      foreach (StateManifest manifest in manifests)
      {
        ArgumentNullException.ThrowIfNull(manifest, nameof(manifests));
        if (!ByAssembly.ContainsKey(manifest.AssemblyName) && ByAssembly.Count >= MaxAssemblies)
        {
          continue;
        }

        ByAssembly[manifest.AssemblyName] = manifest;
        stored++;
      }

      changed = stored > 0 ? Changed : null;
    }

    changed?.Invoke();
    return stored;
  }

  /// <summary>
  /// Copies the stored manifests, ordered by assembly name.
  /// </summary>
  public IReadOnlyList<StateManifest> Snapshot()
  {
    lock (Sync)
    {
      return ByAssembly.Values.OrderBy(manifest => manifest.AssemblyName, StringComparer.Ordinal).ToArray();
    }
  }
}
