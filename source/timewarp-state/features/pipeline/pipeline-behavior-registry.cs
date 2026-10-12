#region Purpose
// Holds the open-generic pipeline behavior types the source generator recorded.
#endregion

#region Design
// LogTimeWarpStateMiddleware lists closed behavior registrations in service-collection order. Walking each
// implementation's interfaces would be reflection. Each compilation that declares a nameable open-generic
// IPipelineBehavior emits a module initializer that records the behavior definition and the interface definition.
// GetComponentOrder compares generic definitions. It does not walk interfaces. A behavior compiled without the
// generator is absent from the log. Open-generic DI registrations are not constructed types, so they stay out.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Open-generic pipeline behaviors recorded by <c>PipelineBehaviorRegistrySourceGenerator</c>.
/// </summary>
public static class PipelineBehaviorRegistry
{
  private static ImmutableDictionary<Type, Type> Definitions =
    ImmutableDictionary<Type, Type>.Empty;

  /// <summary>
  /// Records that <paramref name="behaviorDefinition"/> implements <paramref name="interfaceDefinition"/>.
  /// Both arguments are open generic definitions. A later call for the same behavior replaces the interface.
  /// </summary>
  public static void Register(Type behaviorDefinition, Type interfaceDefinition)
  {
    ArgumentNullException.ThrowIfNull(behaviorDefinition);
    ArgumentNullException.ThrowIfNull(interfaceDefinition);
    ImmutableInterlocked.Update
    (
      ref Definitions,
      static (dictionary, registration) => dictionary.SetItem(registration.Key, registration.Value),
      (Key: behaviorDefinition, Value: interfaceDefinition)
    );
  }

  internal static bool Matches(Type implementationType, Type componentType)
  {
    ArgumentNullException.ThrowIfNull(implementationType);
    ArgumentNullException.ThrowIfNull(componentType);
    if (!implementationType.IsConstructedGenericType)
    {
      return false;
    }

    Type definition = implementationType.GetGenericTypeDefinition();
    ImmutableDictionary<Type, Type> definitions = Definitions;
    return definitions.TryGetValue(definition, out Type? interfaceDefinition)
      && interfaceDefinition == componentType;
  }
}
