namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
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
    serviceCollection.AddTransient<IRequestHandler<StartRequest>, StartHandler>();
    serviceCollection.AddScoped(serviceProvider => (IReduxDevToolsStore)serviceProvider.GetRequiredService<IStore>());

    serviceCollection.AddSingleton(reduxDevToolsOptions);

    return timeWarpStateOptions;
  }
}
