#region Purpose
// AddTimeWarpState: registers the Store, Subscriptions, options and every State<T> found in the configured assemblies.
#endregion

#region Design
// Defaults to the calling assembly when none are configured. Uses TryAdd so hosts can pre-register overrides, adds
// a NullLogger, and registers states as transient. Blazor services (render subscriptions, JavaScript dispatch,
// NavigationManager HttpClient) are AddTimeWarpStateBlazor in TimeWarp.State.Blazor. Nothing mediator-related
// is registered: behaviors are woven at compile time (see assembly-marker.cs).
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  /// <summary>
  /// Register TimeWarp.State services based on the Configure options
  /// </summary>
  /// <param name="serviceCollection"></param>
  /// <param name="configureTimeWarpStateOptionsAction"></param>
  /// <returns></returns>
  /// <example></example>
  /// <remarks>
  /// The order of registration matters.
  /// If the user wants to change the order they can configure themselves vs using this extension
  /// </remarks>
  public static IServiceCollection AddTimeWarpState
  (
    this IServiceCollection serviceCollection,
    Action<TimeWarpStateOptions>? configureTimeWarpStateOptionsAction = null
  )
  {
    var timeWarpStateOptions = new TimeWarpStateOptions(serviceCollection);
    configureTimeWarpStateOptionsAction?.Invoke(timeWarpStateOptions);

    if (!timeWarpStateOptions.Assemblies.Any())
    {
      // If no assemblies are specified then we will use the assembly that called this method.
      // This is to avoid the user having to specify the assembly in the options.
      // If the user specifies any assemblies they will have to specify the calling assembly also if they want it to be used.
      timeWarpStateOptions.Assemblies = [Assembly.GetCallingAssembly()];
    }
    TimeWarpStateOptionsValidator.Validate(timeWarpStateOptions);

    serviceCollection.TryAddScoped<Subscriptions>();
    serviceCollection.TryAddScoped<IStore, Store>();
    serviceCollection.TryAddSingleton(timeWarpStateOptions);

    EnsureLogger(serviceCollection);
    EnsureStates(serviceCollection, timeWarpStateOptions);

    // TimeWarp.Mediator links handlers and pipeline behaviors at compile time. The consuming
    // application references TimeWarp.Mediator.Generators and calls the generated
    // AddGeneratedMediator<ClientPipeline>(); this assembly joins that graph via
    // [assembly: MediatorAssembly], is scoped to the ClientPipeline via [assembly: MediatorScope] and
    // declares its behaviors with [assembly: MediatorBehavior(..., Scope = typeof(ClientPipeline))]
    // (see assembly-marker.cs): state-initialization -> state-transaction.
    // Redux DevTools and render subscriptions are woven by TimeWarp.State.Blazor when that package
    // is referenced. Nothing mediator-related is registered here. UseStateTransactionBehavior is
    // honored at runtime by StateTransactionBehavior itself (it reads TimeWarpStateOptions).

    return serviceCollection;
  }

  /// <summary>
  /// If no ILogger is registered it would throw as we inject it.  This provides us with a NullLogger to avoid that
  /// </summary>
  /// <param name="serviceCollection"></param>
  private static void EnsureLogger(IServiceCollection serviceCollection)
  {
    serviceCollection.TryAddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
  }

  // Known trim-unsafe assembly scan, kept until states are registered by generated code (follow-up in task 097 Results).
  // Blazor and console hosts do not trim application assemblies by default (TrimMode=partial), so the scanned state
  // types and their constructors survive; a host that trims its own assembly must root its states.
  [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "State discovery scans the configured application assemblies, which are not trimmed by default. Follow-up: generated state registration.")]
  [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "State types come from the application assembly scan; their public constructors are kept because application assemblies are not trimmed by default. Follow-up: generated state registration.")]
  private static void EnsureStates(IServiceCollection serviceCollection, TimeWarpStateOptions timeWarpStateOptions)
  {
    foreach (Assembly assembly in timeWarpStateOptions.Assemblies)
    {
      IEnumerable<Type> types = assembly.GetTypes().Where
      (
        type =>
          type is { IsAbstract: false, IsInterface: false } &&
          type.BaseType != null &&
          IsDerivedFromGenericType(type.BaseType, typeof(State<>))
      );

      foreach (Type type in types)
      {
        serviceCollection.TryAddTransient(type);
      }
    }
    return;

    bool IsDerivedFromGenericType(Type type, Type genericType)
    {
      Type? currentType = type;

      while (currentType != null && currentType != typeof(object))
      {
        if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericType)
        {
          return true;
        }
        currentType = currentType.BaseType;
      }
      return false;
    }
  }
}
