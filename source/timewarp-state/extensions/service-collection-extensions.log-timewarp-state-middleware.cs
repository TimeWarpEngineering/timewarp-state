#region Purpose
// Logs the TimeWarp.Mediator pipeline behaviors registered in the service collection, in pipeline order.
#endregion

#region Design
// Only closed constructed behavior registrations are listed, because those are what the generated mediator
// registers and runs. Open-generic registrations are inert and would misreport the pipeline.
// The behavior's open definition comes from PipelineBehaviorRegistry, which each compilation fills with a
// module initializer. The list does not walk the implementation's interfaces.
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  public static void LogTimeWarpStateMiddleware(this IServiceCollection serviceCollection, ILogger logger)
  {
    // TimeWarp.Mediator's generator registers each closed behavior it wove (AddScoped<Behavior<TRequest,TResponse>>)
    // in compile-time pipeline order, so the distinct open-generic names, in registration order, reflect the pipeline.
    // Open-generic IPipelineBehavior<,> registrations are ignored: the generated mediator never runs them.
    List<string> middleware = GetComponentOrder(serviceCollection, typeof(IPipelineBehavior<,>));

    var message = new StringBuilder("TimeWarp State (TimeWarp.Mediator) Pipeline Behavior Registrations:");
    message.AppendLine();
    message.AppendLine();

    AppendComponentOrder(message, "Behaviors (in pipeline order)", middleware);

    logger.LogInformation(message.ToString());
  }

  // Only closed constructed implementation types are considered: those are what TimeWarp.Mediator's generator
  // registers and runs. A legacy open-generic registration (AddScoped(typeof(IPipelineBehavior<,>), typeof(X<,>)))
  // is inert under the generated mediator, so listing it here would misreport the pipeline.
  // PipelineBehaviorRegistry.Matches compares open definitions the source generator recorded. It does not walk
  // the implementation's interfaces.
  /// <summary>
  /// Lists the distinct simple names of the closed implementation types registered for <paramref name="componentType"/>, in registration order.
  /// </summary>
  /// <remarks>
  /// Only behaviors the source generator recorded for <c>IPipelineBehavior&lt;,&gt;</c> are matched.
  /// Any other component type returns an empty list.
  /// </remarks>
  public static List<string> GetComponentOrder(this IServiceCollection serviceCollection, Type componentType)
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);
    ArgumentNullException.ThrowIfNull(componentType);
    return serviceCollection
      .Select(descriptor => descriptor.ImplementationType)
      .Where(type => type is not null && PipelineBehaviorRegistry.Matches(type, componentType))
      .Select(type => type!.GetSimpleName())
      .Distinct()
      .ToList();
  }

  private static void AppendComponentOrder(StringBuilder message, string componentType, IReadOnlyList<string> order)
  {
    message.AppendLine($"{componentType}:");
    for (int i = 0; i < order.Count; i++)
    {
      message.AppendLine($"  {i + 1}. {order[i]}");
    }
    message.AppendLine();
  }
}
