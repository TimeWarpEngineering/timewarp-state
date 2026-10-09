#region Purpose
// AddActionCatalog: registers IActionCatalog and adds the generated [CatalogAction] entries from named assemblies.
#endregion

#region Design
// Reads each assembly's ActionCatalogProviderAttribute (emitted by the source generator) instead of scanning
// types. Each source is a singleton and IActionCatalog is TryAdd'ed, so the method can be called repeatedly.
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers <see cref="IActionCatalog"/> and adds the generated action catalog of each assembly.
  /// </summary>
  /// <param name="serviceCollection">The service collection.</param>
  /// <param name="assemblies">
  /// Assemblies whose <c>[CatalogAction]</c> actions to include, for example <c>typeof(Marker).Assembly</c>.
  /// Assemblies without cataloged actions contribute nothing. Call again to add more.
  /// </param>
  /// <remarks>
  /// No global scanning: only the named assemblies are read. Permissions on entries are not enforced here.
  /// </remarks>
  public static IServiceCollection AddActionCatalog
  (
    this IServiceCollection serviceCollection,
    params Assembly[] assemblies
  )
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);
    ArgumentNullException.ThrowIfNull(assemblies);

    foreach (Assembly assembly in assemblies)
    {
      ActionCatalogProviderAttribute? provider = assembly.GetCustomAttribute<ActionCatalogProviderAttribute>();
      if (provider is not null)
      {
        serviceCollection.AddSingleton(new ActionCatalogSource(provider.Entries));
      }
    }

    serviceCollection.TryAddSingleton<IActionCatalog, ActionCatalog>();
    return serviceCollection;
  }
}
