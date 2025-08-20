namespace TimeWarp.State.Plus.Extensions;

using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Features.Routing;
using static TimeWarp.Features.Routing.RouteState;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Register TimeWarp.State.Plus Routing 
  /// </summary>
  /// <param name="serviceCollection"></param>
  public static IServiceCollection AddTimeWarpStateRouting(this IServiceCollection serviceCollection)
  {
    // To avoid duplicate registrations we look to see if one has already been registered.
    if (serviceCollection.HasRegistrationFor(typeof(RouteState))) return serviceCollection;

    serviceCollection.AddScoped<RouteState>();
    serviceCollection.AddTransient<IRequestHandler<ChangeRouteActionSet.Action>, ChangeRouteActionSet.Handler>();
    serviceCollection.AddTransient<IRequestHandler<GoBackActionSet.Action>, GoBackActionSet.Handler>();
    serviceCollection.AddTransient<IRequestHandler<PushRouteInfoActionSet.Action>, PushRouteInfoActionSet.Handler>();

    return serviceCollection;
  }
  
  private static bool HasRegistrationFor(this IServiceCollection serviceCollection, Type type) =>
    serviceCollection.Any(serviceDescriptor => serviceDescriptor.ServiceType == type);
  
}
