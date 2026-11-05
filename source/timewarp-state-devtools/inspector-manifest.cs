#region Purpose
// Process-wide registry of the State manifests the inspected apps posted, one per assembly.
#endregion

#region Design
// A singleton like InspectorLog, so the ingest request and every circuit see the same list. The latest manifest
// for an assembly name replaces the earlier one, so a restarted app does not pile up duplicates. The count is
// capped because the input is untrusted: past MaxAssemblies a new assembly name is refused, while an assembly
// already known can still be replaced. Changed is raised outside the lock, and only when something changed.
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
