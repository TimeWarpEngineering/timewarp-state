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
    // The routing action handlers (ChangeRoute/GoBack/PushRouteInfo) are linked by the consuming
    // application's generated AddGeneratedMediator<ClientPipeline>(): this assembly carries
    // [assembly: MediatorAssembly] and [assembly: MediatorScope(typeof(ClientPipeline))], so the
    // TimeWarp.Mediator generator discovers them at compile time and no manual registration is needed.

    return serviceCollection;
  }
  
  private static bool HasRegistrationFor(this IServiceCollection serviceCollection, Type type) =>
    serviceCollection.Any(serviceDescriptor => serviceDescriptor.ServiceType == type);
  
}
