#region Purpose
// Process-wide opt-in that allows test-only state entry points.
#endregion

#region Design
// ThrowIfNotTestAssembly reads AllowTestAccess before the assembly-name fallback.
// Enable() is the contract a test host calls. Reset() is internal so unit tests
// can isolate the flag; production hosts call Enable() only.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Opts this process in to test-only state entry points.
/// </summary>
public static class StateTestOptions
{
  /// <summary>
  /// Gets a value indicating whether test-only state entry points are allowed
  /// without an assembly-name check.
  /// </summary>
  public static bool AllowTestAccess { get; private set; }

  /// <summary>
  /// Allows test-only state entry points for this process.
  /// </summary>
  public static void Enable() => AllowTestAccess = true;

  internal static void Reset() => AllowTestAccess = false;
}
