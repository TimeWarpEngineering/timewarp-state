#region Purpose
// Aggregates per-assembly generated registries into one IActionCatalog.
#endregion

#region Design
// Each AddActionCatalog call registers one ActionCatalogSource per assembly. Duplicate names across
// assemblies throw here; TWS0006 catches duplicates within one assembly at compile time.
#endregion

namespace TimeWarp.State;

/// <summary>
/// One assembly's generated entries, registered by
/// <see cref="ServiceCollectionExtensions.AddActionCatalog(IServiceCollection, Assembly[])"/>.
/// </summary>
/// <param name="Entries">The assembly's cataloged actions.</param>
public sealed record ActionCatalogSource(IReadOnlyList<ActionCatalogEntry> Entries);

/// <summary>
/// Default <see cref="IActionCatalog"/> over every registered <see cref="ActionCatalogSource"/>.
/// </summary>
public sealed class ActionCatalog : IActionCatalog
{
  private readonly Dictionary<string, ActionCatalogEntry> EntriesByName;

  /// <summary>
  /// Builds the catalog. Throws when two assemblies declare the same catalog name.
  /// </summary>
  public ActionCatalog(IEnumerable<ActionCatalogSource> sources)
  {
    ArgumentNullException.ThrowIfNull(sources);
    List<ActionCatalogEntry> entries = [];
    EntriesByName = new Dictionary<string, ActionCatalogEntry>(StringComparer.Ordinal);

    foreach (ActionCatalogEntry entry in sources.SelectMany(source => source.Entries))
    {
      if (!EntriesByName.TryAdd(entry.Name, entry))
      {
        if (ReferenceEquals(EntriesByName[entry.Name], entry)) continue;
        throw new InvalidOperationException
        (
          $"Action catalog name '{entry.Name}' is declared by both {EntriesByName[entry.Name].ActionType} and {entry.ActionType}."
        );
      }

      entries.Add(entry);
    }

    Entries = entries;
  }

  /// <inheritdoc />
  public IReadOnlyList<ActionCatalogEntry> Entries { get; }

  /// <inheritdoc />
  public ActionCatalogEntry? Find(string name) => EntriesByName.GetValueOrDefault(name);
}
