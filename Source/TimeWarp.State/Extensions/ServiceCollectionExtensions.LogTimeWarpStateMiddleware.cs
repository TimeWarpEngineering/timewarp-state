namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  public static void LogTimeWarpStateMiddleware(this IServiceCollection serviceCollection, ILogger logger)
  {
    List<string> preprocessors = GetComponentOrder(serviceCollection, typeof(IRequestPreProcessor<>));
    List<string> middleware = GetComponentOrder(serviceCollection, typeof(IPipelineBehavior<,>));
    List<string> postprocessors = GetComponentOrder(serviceCollection, typeof(IRequestPostProcessor<,>));

    LogOrder(logger, "MediatR preprocessors", preprocessors);
    LogOrder(logger, "MediatR middleware", middleware);
    LogOrder(logger, "MediatR postprocessors", postprocessors);
  }

  public static List<string> GetComponentOrder(this IServiceCollection serviceCollection, Type componentType)
  {
    return serviceCollection
      .Where(sd => sd.ServiceType.IsGenericType && 
        sd.ServiceType.GetGenericTypeDefinition() == componentType)
      .Select(sd => sd.ImplementationType?.Name ?? "Unknown")
      .ToList();
  }

  private static void LogOrder(ILogger logger, string componentType, IReadOnlyList<string> order)
  {
    var message = new StringBuilder($"{componentType} registration order:\n");
    for (int i = 0; i < order.Count; i++)
    {
      message.AppendLine($"{i + 1}. {order[i]}");
    }
    logger.LogInformation(message.ToString());
  }
}
