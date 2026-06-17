namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  public static void LogTimeWarpStateMiddleware(this IServiceCollection serviceCollection, ILogger logger)
  {
    // With Mediator (martinothamar), pre- and post-processors are themselves IPipelineBehavior
    // implementations (MessagePreProcessor / MessagePostProcessor), so a single ordered list of
    // IPipelineBehavior registrations reflects the full pipeline.
    List<string> middleware = GetComponentOrder(serviceCollection, typeof(IPipelineBehavior<,>));

    var message = new StringBuilder("TimeWarp State (Mediator) Pipeline Behavior Registrations:");
    message.AppendLine();
    message.AppendLine();

    AppendComponentOrder(message, "Behaviors (in pipeline order)", middleware);

    logger.LogInformation(message.ToString());
  }

  public static List<string> GetComponentOrder(this IServiceCollection serviceCollection, Type componentType)
  {
    return serviceCollection
      .Where(sd => sd.ServiceType.IsGenericType && 
        sd.ServiceType.GetGenericTypeDefinition() == componentType)
      .Select(sd => sd.ImplementationType?.Name ?? "Unknown")
      .Select(name => name.Split('`')[0])
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
