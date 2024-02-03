namespace BlazorState;

using static BlazorState.Features.Routing.RouteState;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Register BlazorState services based on the aConfigure options
  /// </summary>
  /// <param name="aServiceCollection"></param>
  /// <param name="aConfigureBlazorStateOptionsAction"></param>
  /// <returns></returns>
  /// <example></example>
  /// <remarks>
  /// The order of registration matters.
  /// If the user wants to change the order they can configure themselves vs using this extension
  /// </remarks>
  public static IServiceCollection AddBlazorState
  (
    this IServiceCollection aServiceCollection,
    Action<BlazorStateOptions> aConfigureBlazorStateOptionsAction = null
  )
  {
    // To avoid duplicate registrations we look to see if Subscriptions has already been registered.
    if (aServiceCollection.HasRegistrationFor(typeof(Subscriptions))) return aServiceCollection;
    
    var blazorStateOptions = new BlazorStateOptions(aServiceCollection);
    aConfigureBlazorStateOptionsAction?.Invoke(blazorStateOptions);

    aServiceCollection.AddScoped<BlazorHostingLocation>();
    aServiceCollection.AddScoped<JsonRequestHandler>();
    aServiceCollection.AddScoped<Subscriptions>();
    aServiceCollection.AddScoped<IStore, Store>();
    aServiceCollection.AddSingleton(blazorStateOptions);
      
    EnsureLogger(aServiceCollection);
    EnsureHttpClient(aServiceCollection);
    EnsureStates(aServiceCollection, blazorStateOptions);
    EnsureMediator(aServiceCollection, blazorStateOptions);

    if (blazorStateOptions.UseCloneStateBehavior)
    {
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
    }

    if (blazorStateOptions.UseRouting)
    {
      aServiceCollection.AddScoped<TimeWarpNavigationManager>();
      aServiceCollection.AddScoped<RouteState>();

      aServiceCollection.AddTransient<IRequestHandler<ChangeRouteAction>, ChangeRouteHandler>();
      aServiceCollection.AddTransient<IRequestHandler<GoBackAction>, GoBackHandler>();
      aServiceCollection.AddTransient<IRequestHandler<InitializeRouteAction>, InitializeRouteHandler>();
    }
    return aServiceCollection;
  }

  private static void EnsureHttpClient(IServiceCollection aServiceCollection)
  {
    var blazorHostingLocation = new BlazorHostingLocation();

    // Test.App.Server Side Blazor doesn't register HttpClient by default
    if (!blazorHostingLocation.IsServerSide) return;
    
    // Double check that nothing is registered.
    if (aServiceCollection.HasRegistrationFor(typeof(HttpClient))) return;
    
    // Setup HttpClient for server side in a client side compatible fashion
    aServiceCollection.AddScoped<HttpClient>
    (
      aServiceProvider =>
      {
        // Creating the NavigationManager needs to wait until the JS Runtime is initialized, so defer it.
        NavigationManager navigationManager = aServiceProvider.GetRequiredService<NavigationManager>();

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
  /// <param name="aServiceCollection"></param>
  private static void EnsureLogger(IServiceCollection aServiceCollection)
  {
    if(!aServiceCollection.HasRegistrationFor(typeof(ILogger<>)))
    {
      aServiceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
    }
  }

  /// <summary>
  /// Scan Assemblies for Handlers.
  /// </summary>
  /// <param name="aServiceCollection"></param>
  /// <param name="aBlazorStateOptions"></param>
  private static void EnsureMediator(IServiceCollection aServiceCollection, BlazorStateOptions aBlazorStateOptions)
  {
    if (aServiceCollection.HasRegistrationFor(typeof(IMediator))) return;
    
    aServiceCollection
      .AddMediatR
      (
      aMediatRServiceConfiguration =>
        aMediatRServiceConfiguration
          .RegisterServicesFromAssemblies(aBlazorStateOptions.Assemblies.ToArray())
          .AddOpenRequestPostProcessor(typeof(RenderSubscriptionsPostProcessor<,>))
      );
    aServiceCollection.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>), ServiceLifetime.Transient));
  }

  private static void EnsureStates(IServiceCollection aServiceCollection, BlazorStateOptions aBlazorStateOptions)
  {
    foreach (Assembly assembly in aBlazorStateOptions.Assemblies)
    {
      IEnumerable<Type> types = assembly.GetTypes().Where
      (
        aType =>
          !aType.IsAbstract &&
          !aType.IsInterface &&
          aType.BaseType != null &&
          aType.BaseType.IsGenericType &&
          aType.BaseType.GetGenericTypeDefinition() == typeof(State<>)
      );

      foreach (Type type in types)
      {
        aServiceCollection.TryAddTransient(type);
      }
    }
  }

  private static bool HasRegistrationFor(this IServiceCollection aServiceCollection, Type aType) => 
    aServiceCollection.Any(aServiceDescriptor => aServiceDescriptor.ServiceType == aType);

  public static BlazorStateOptions UseReduxDevTools
  (
    this BlazorStateOptions aBlazorStateOptions,
    Action<ReduxDevToolsOptions> aReduxDevToolsOptionsAction = null
  )
  {
    IServiceCollection serviceCollection = aBlazorStateOptions.ServiceCollection;
    if (serviceCollection.HasRegistrationFor(typeof(ReduxDevToolsOptions))) return aBlazorStateOptions;
    
    var reduxDevToolsOptions = new ReduxDevToolsOptions();
    aReduxDevToolsOptionsAction?.Invoke(reduxDevToolsOptions);

    serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(ReduxDevToolsBehavior<,>));
    serviceCollection.AddScoped<ReduxDevToolsInterop>();

    serviceCollection.AddTransient<IRequestHandler<CommitRequest>, CommitHandler>();
    serviceCollection.AddTransient<IRequestHandler<JumpToStateRequest>, JumpToStateHandler>();
    serviceCollection.AddTransient<IRequestHandler<StartRequest>, StartHandler>();
    serviceCollection.AddScoped(aServiceProvider => (IReduxDevToolsStore)aServiceProvider.GetService<IStore>());

    serviceCollection.AddSingleton<ReduxDevToolsOptions>(reduxDevToolsOptions);

    return aBlazorStateOptions;
  }
}
