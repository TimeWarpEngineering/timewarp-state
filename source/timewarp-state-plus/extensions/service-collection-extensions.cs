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
    // The routing action handlers (ChangeRoute/GoBack/PushRouteInfo) are registered by the consuming
    // application's AddMediator(...) call via the TimeWarp.State.Plus assembly marker; Mediator's source
    // generator discovers them at compile time, so manual handler registration here is no longer needed.

    return serviceCollection;
  }
  
  private static bool HasRegistrationFor(this IServiceCollection serviceCollection, Type type) =>
    serviceCollection.Any(serviceDescriptor => serviceDescriptor.ServiceType == type);
  
}
