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
  public static List<string> GetComponentOrder(this IServiceCollection serviceCollection, Type componentType)
  {
    return serviceCollection
      .Where
      (
        sd =>
          sd.ImplementationType is { IsConstructedGenericType: true } implementationType &&
          implementationType.GetInterfaces().Any
          (
            i => i.IsGenericType && i.GetGenericTypeDefinition() == componentType
          )
      )
      .Select(sd => sd.ImplementationType!.Name.Split('`')[0])
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
