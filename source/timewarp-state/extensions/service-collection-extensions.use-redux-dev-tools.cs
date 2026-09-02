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

    // ReduxDevToolsBehavior is woven at compile time ([assembly: MediatorBehavior] in
    // assembly-marker.cs) and CommitHandler/StartHandler are linked by the host's generator.
    // Registering ReduxDevToolsOptions here is what switches the behavior on: it resolves the
    // options as an optional dependency and is a pass-through when UseReduxDevTools was not called.
    serviceCollection.AddScoped<ReduxDevToolsInterop>();
    serviceCollection.AddScoped(serviceProvider => (IReduxDevToolsStore)serviceProvider.GetRequiredService<IStore>());

    serviceCollection.AddSingleton(reduxDevToolsOptions);

    return timeWarpStateOptions;
  }
}
