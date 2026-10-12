#region Purpose
// Holds the per-assembly state DI registrations the clone source generator emits.
#endregion

#region Design
// Each compilation that contains a registerable State<T> emits one module initializer. The initializer stores a
// delegate under that assembly. AddTimeWarpState applies the delegates for TimeWarpStateOptions.Assemblies.
// The delegate calls TryAddTransient<T>() for each concrete state, so the trimmer sees the closed type. There is
// no type scan and no untyped transient registration. A host registration wins because the generated calls
// use TryAdd. An assembly with no generated states contributes nothing.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Generated <see cref="State{TState}"/> DI registrations, keyed by the assembly that declared them.
/// </summary>
public static class StateRegistrationRegistry
{
  private static ImmutableDictionary<Assembly, Action<IServiceCollection>> Registrars =
    ImmutableDictionary<Assembly, Action<IServiceCollection>>.Empty;

  /// <summary>
  /// Stores the registration delegate for <paramref name="assembly"/>.
  /// A later call for the same assembly replaces the previous delegate.
  /// </summary>
  public static void Register(Assembly assembly, Action<IServiceCollection> register)
  {
    ArgumentNullException.ThrowIfNull(assembly);
    ArgumentNullException.ThrowIfNull(register);
    ImmutableInterlocked.Update
    (
      ref Registrars,
      static (dictionary, registration) => dictionary.SetItem(registration.Key, registration.Value),
      (Key: assembly, Value: register)
    );
  }

  internal static void Apply(IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies)
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);
    ArgumentNullException.ThrowIfNull(assemblies);
    ImmutableDictionary<Assembly, Action<IServiceCollection>> registrars = Registrars;
    foreach (Assembly assembly in assemblies)
    {
      if (registrars.TryGetValue(assembly, out Action<IServiceCollection>? register))
      {
        register(serviceCollection);
      }
    }
  }
}
