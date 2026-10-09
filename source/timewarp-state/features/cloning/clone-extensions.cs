#region Purpose
// Public entry point for TimeWarp.State's non-blocking deep clone.
#endregion

#region Design
// Opt-in namespace so the generic extension does not appear on every type for consumers who only import TimeWarp.State.
// Instance Clone() methods (ICloneable) still win over this extension at the call site.
// Migrating from AnyClone: replace `using AnyClone;` with `using TimeWarp.Features.Cloning;`.
#endregion

namespace TimeWarp.Features.Cloning;

/// <summary>
/// Deep clone without blocking waits, safe on single-threaded browser WebAssembly.
/// </summary>
public static class CloneExtensions
{
  /// <summary>
  /// Returns a deep copy of <paramref name="source"/>. Members marked [IgnoreDataMember], [NonSerialized] or
  /// [JsonIgnore] are not copied; the clone keeps the values its constructor assigned.
  /// </summary>
  /// <exception cref="Exception">Any failure while cloning a member is rethrown.</exception>
  public static T Clone<T>(this T source) => DeepCloner.Clone(source, onError: null);

  /// <summary>
  /// Returns a deep copy of <paramref name="source"/>, reporting members that cannot be cloned to
  /// <paramref name="onError"/> instead of throwing.
  /// </summary>
  public static T Clone<T>(this T source, CloneErrorHandler onError)
  {
    ArgumentNullException.ThrowIfNull(onError);
    return DeepCloner.Clone(source, onError);
  }
}
