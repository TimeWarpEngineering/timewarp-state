#region Purpose
// Assembly-level hook the generator emits so AddActionCatalog(assembly) can find an assembly's entries.
#endregion

#region Design
// The generator emits an internal sealed subclass whose Entries returns the generated registry and
// applies it with [assembly: ...]. Reading an assembly attribute instance needs no member reflection,
// so aggregation stays AOT/trim safe and there is no global static scanning: the host names each
// assembly it wants.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Base for the generated assembly attribute that exposes an assembly's action catalog.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public abstract class ActionCatalogProviderAttribute : Attribute
{
  /// <summary>Cataloged actions declared in the assembly.</summary>
  public abstract IReadOnlyList<ActionCatalogEntry> Entries { get; }
}
