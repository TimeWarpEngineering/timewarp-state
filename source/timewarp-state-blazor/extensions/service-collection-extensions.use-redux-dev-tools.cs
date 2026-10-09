#region Purpose
// UseReduxDevTools: turns on the Redux DevTools integration for TimeWarp.State.
#endregion

#region Design
// The behavior is always woven. Registering ReduxDevToolsOptions is what switches it on. The method is idempotent
// (returns early if already registered) and allow-lists the DevTools Start/Commit requests only when enabled.
#endregion

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
    // TimeWarp.State.Blazor assembly-marker.cs) and CommitHandler/StartHandler are linked by the host's generator.
    // Registering ReduxDevToolsOptions here is what switches the behavior on: it resolves the
    // options as an optional dependency and is a pass-through when UseReduxDevTools was not called.
    serviceCollection.AddScoped<ReduxDevToolsInterop>();
    serviceCollection.AddScoped(serviceProvider => (IReduxDevToolsStore)serviceProvider.GetRequiredService<IStore>());

    serviceCollection.AddSingleton(reduxDevToolsOptions);

    // DevTools messages (Start, Commit) reach .NET through JsonRequestHandler. They are not
    // actions, so they are allow-listed here and only when DevTools is enabled.
    JavaScriptDispatchRegistry.GetOrAdd(serviceCollection).AllowReduxDevToolsRequests();

    return timeWarpStateOptions;
  }

  private static bool HasRegistrationFor(this IServiceCollection serviceCollection, Type type) =>
    serviceCollection.Any(serviceDescriptor => serviceDescriptor.ServiceType == type);
}
