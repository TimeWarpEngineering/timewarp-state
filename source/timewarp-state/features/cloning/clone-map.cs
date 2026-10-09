#region Purpose
// Per-clone identity map so cycles and shared references stay one instance.
#endregion

#region Design
// Reference equality, allocated by the caller for one clone and discarded with it. No pool and no lock: a clone
// must stay non-blocking on single-threaded browser WebAssembly. The generated cloner inserts an instance before
// copying its members.
#endregion

namespace TimeWarp.Features.Cloning;

/// <summary>
/// Maps an original reference to the clone created for it during one clone operation.
/// </summary>
public sealed class CloneMap
{
  private readonly Dictionary<object, object> Visited = new(ReferenceEqualityComparer.Instance);

  /// <summary>
  /// Returns the clone already created for <paramref name="source"/> in this operation.
  /// </summary>
  public bool TryGet<T>(object source, out T? existing)
    where T : class
  {
    ArgumentNullException.ThrowIfNull(source);
    if (Visited.TryGetValue(source, out object? found) && found is T typed)
    {
      existing = typed;
      return true;
    }

    existing = null;
    return false;
  }

  /// <summary>
  /// Records <paramref name="clone"/> as the copy of <paramref name="source"/>.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="source"/> was already recorded.</exception>
  public void Add(object source, object clone)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(clone);
    Visited.Add(source, clone);
  }
}
