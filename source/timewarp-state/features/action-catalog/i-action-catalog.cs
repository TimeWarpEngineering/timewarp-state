#region Purpose
// Read-only access to every cataloged action from the assemblies registered with AddActionCatalog.
#endregion

#region Design
// Small interface (Entries in registration order, Find by ordinal name) so consumers and tests depend on the
// abstraction, not the ActionCatalog implementation.
#endregion

namespace TimeWarp.State;

/// <summary>
/// All cataloged actions from the assemblies registered with
/// <see cref="ServiceCollectionExtensions.AddActionCatalog(IServiceCollection, Assembly[])"/>.
/// </summary>
public interface IActionCatalog
{
  /// <summary>Entries in registration order.</summary>
  IReadOnlyList<ActionCatalogEntry> Entries { get; }

  /// <summary>Finds an entry by its catalog name (ordinal).</summary>
  ActionCatalogEntry? Find(string name);
}
