namespace TimeWarp.State;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Register TimeWarp.State services based on the Configure options
  /// </summary>
  /// <param name="serviceCollection"></param>
  /// <param name="configureBlazorStateOptionsAction"></param>
  /// <returns></returns>
  /// <example></example>
  /// <remarks>
  /// The order of registration matters.
  /// If the user wants to change the order they can configure themselves vs using this extension
  /// </remarks>
  public static IServiceCollection AddTimeWarpState
  (
    this IServiceCollection serviceCollection,
    Action<TimeWarpStateOptions>? configureBlazorStateOptionsAction = null
  )
  {
    // To avoid duplicate registrations we look to see if Subscriptions has already been registered.
    if (serviceCollection.HasRegistrationFor(typeof(Subscriptions))) return serviceCollection;

    var blazorStateOptions = new TimeWarpStateOptions(serviceCollection);
    configureBlazorStateOptionsAction?.Invoke(blazorStateOptions);

    serviceCollection.AddScoped<JsonRequestHandler>();
    serviceCollection.AddScoped<Subscriptions>();
    serviceCollection.AddScoped<IStore, Store>();
    serviceCollection.AddSingleton(blazorStateOptions);

    EnsureLogger(serviceCollection);
    EnsureHttpClient(serviceCollection);
    EnsureStates(serviceCollection, blazorStateOptions);
    EnsureMediator(serviceCollection, blazorStateOptions);

    if (blazorStateOptions.UseStateTransactionBehavior)
    {
      serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(StateTransactionBehavior<,>));
    }

    return serviceCollection;
  }

  private static void EnsureHttpClient(IServiceCollection serviceCollection)
  {
    // If client side wasm, Blazor registers HttpClient by default.
    if (OperatingSystem.IsBrowser()) return;

    // Double check that nothing is registered.
    if (serviceCollection.HasRegistrationFor(typeof(HttpClient))) return;

    // Setup HttpClient for server side in a client side compatible fashion
    serviceCollection.AddScoped
    (
      serviceProvider =>
      {
        // Creating the NavigationManager needs to wait until the JS Runtime is initialized, so defer it.
        NavigationManager navigationManager = serviceProvider.GetRequiredService<NavigationManager>();

        return new HttpClient
        {
          BaseAddress = new Uri(navigationManager.BaseUri)
        };
      }
    );
  }

  /// <summary>
  /// If no ILogger is registered it would throw as we inject it.  This provides us with a NullLogger to avoid that
  /// </summary>
  /// <param name="serviceCollection"></param>
  private static void EnsureLogger(IServiceCollection serviceCollection)
  {
    if (!serviceCollection.HasRegistrationFor(typeof(ILogger<>)))
    {
      serviceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
    }
  }

  /// <summary>
  /// Scan Assemblies for Handlers.
  /// </summary>
  /// <param name="serviceCollection"></param>
  /// <param name="timeWarpStateOptions"></param>
  private static void EnsureMediator(IServiceCollection serviceCollection, TimeWarpStateOptions timeWarpStateOptions)
  {
    if (serviceCollection.HasRegistrationFor(typeof(IMediator))) return;

    serviceCollection
      .AddMediatR
      (
        mediatRServiceConfiguration =>
          mediatRServiceConfiguration
            .RegisterServicesFromAssemblies(timeWarpStateOptions.Assemblies.ToArray())
            .AddOpenRequestPostProcessor(typeof(RenderSubscriptionsPostProcessor<,>))
      );
    serviceCollection.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>), ServiceLifetime.Transient));
  }

  private static void EnsureStates(IServiceCollection serviceCollection, TimeWarpStateOptions timeWarpStateOptions)
  {
    foreach (Assembly assembly in timeWarpStateOptions.Assemblies)
    {
      IEnumerable<Type> types = assembly.GetTypes().Where
      (
        type =>
          type is { IsAbstract: false, IsInterface: false, BaseType.IsGenericType: true } &&
          type.BaseType.GetGenericTypeDefinition() == typeof(State<>)
      );

      foreach (Type type in types)
      {
        serviceCollection.TryAddTransient(type);
      }
    }
  }

  private static bool HasRegistrationFor(this IServiceCollection serviceCollection, Type type) =>
    serviceCollection.Any(serviceDescriptor => serviceDescriptor.ServiceType == type);

  // ReSharper disable once UnusedMethodReturnValue.Global
  public static TimeWarpStateOptions UseReduxDevTools
  (
    this TimeWarpStateOptions timeWarpStateOptions,
    Action<ReduxDevToolsOptions>? reduxDevToolsOptionsAction = null
  )
  {
    IServiceCollection serviceCollection = timeWarpStateOptions.ServiceCollection;
    if (serviceCollection.HasRegistrationFor(typeof(ReduxDevToolsOptions))) return timeWarpStateOptions;

    var reduxDevToolsOptions = new ReduxDevToolsOptions();
    reduxDevToolsOptionsAction?.Invoke(reduxDevToolsOptions);

    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(ReduxDevToolsBehavior<,>));
    serviceCollection.AddScoped<ReduxDevToolsInterop>();

    serviceCollection.AddTransient<IRequestHandler<CommitRequest>, CommitHandler>();
    serviceCollection.AddTransient<IRequestHandler<JumpToStateRequest>, JumpToStateHandler>();
    serviceCollection.AddTransient<IRequestHandler<StartRequest>, StartHandler>();
    serviceCollection.AddScoped(serviceProvider => (IReduxDevToolsStore)serviceProvider.GetRequiredService<IStore>());

    serviceCollection.AddSingleton(reduxDevToolsOptions);

    return timeWarpStateOptions;
  }
}
