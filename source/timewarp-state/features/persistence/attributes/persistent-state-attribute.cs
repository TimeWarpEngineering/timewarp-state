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
