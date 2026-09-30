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
