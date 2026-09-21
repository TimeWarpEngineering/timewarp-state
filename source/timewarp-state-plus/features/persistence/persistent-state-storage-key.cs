#region Purpose
// Resolves browser-storage keys for [PersistentState] types.
#endregion

#region Design
// Writes always use Type.FullName so two states with the same simple name do not share a slot.
// Load tries FullName first, then Type.Name, so entries written under the old simple-name key still hydrate.
#endregion

namespace TimeWarp.Features.Persistence;

internal static class PersistentStateStorageKey
{
  public static string ForWrite(Type stateType)
  {
    ArgumentNullException.ThrowIfNull(stateType);
    return stateType.FullName ??
      throw new InvalidOperationException("The type provided has a null full name, which is not supported for persistence operations.");
  }
}
