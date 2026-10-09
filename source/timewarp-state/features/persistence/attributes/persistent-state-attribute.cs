#region Purpose
// Marks a state class for automatic persistence and says where it is stored (PersistentStateMethod).
#endregion

#region Design
// Single-use class attribute. The persistence behavior itself is implemented by TimeWarp.State.Plus, which keys
// entries by FullName and falls back to Name on load.
#endregion

namespace TimeWarp.Features.Persistence;

/// <summary>
/// Marks a state type for automatic persistence to browser storage.
/// </summary>
/// <remarks>
/// TimeWarp.State.Plus writes under the state's <c>FullName</c> and, on load, tries FullName
/// then the simple <c>Name</c> so existing session/local entries are not dropped.
/// JSON uses <c>TimeWarpStateOptions.JsonSerializerOptions</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PersistentStateAttribute
(
  PersistentStateMethod PersistentStateMethod
) : Attribute
{
  public readonly PersistentStateMethod PersistentStateMethod = PersistentStateMethod;
}
