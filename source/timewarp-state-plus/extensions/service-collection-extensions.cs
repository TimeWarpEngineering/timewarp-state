#region Purpose
// AddTimeWarpStateRouting: registers RouteState for TimeWarp.State.Plus routing.
#endregion

#region Design
// Only RouteState is registered (TryAddScoped). The routing handlers are linked at compile time by the host's
// generated mediator, so they need no registration.
#endregion

namespace TimeWarp.State.Plus.Extensions;

using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Features.Routing;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Register TimeWarp.State.Plus Routing 
  /// </summary>
  /// <param name="serviceCollection"></param>
  public static IServiceCollection AddTimeWarpStateRouting(this IServiceCollection serviceCollection)
  {
    serviceCollection.TryAddScoped<RouteState>();
    // The routing action handlers (ChangeRoute/GoBack/PushRouteInfo) are linked by the consuming
    // application's generated AddGeneratedMediator<ClientPipeline>(): this assembly carries
    // [assembly: MediatorAssembly] and [assembly: MediatorScope(typeof(ClientPipeline))], so the
    // TimeWarp.Mediator generator discovers them at compile time and no manual registration is needed.

    return serviceCollection;
  }
}
