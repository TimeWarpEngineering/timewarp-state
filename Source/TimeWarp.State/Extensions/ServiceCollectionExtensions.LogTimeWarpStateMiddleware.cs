namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  public static void LogTimeWarpStateMiddleware(this IServiceCollection serviceCollection, ILogger logger)
  {
    List<string> preprocessors = GetComponentOrder(serviceCollection, typeof(IRequestPreProcessor<>));
    List<string> middleware = GetComponentOrder(serviceCollection, typeof(IPipelineBehavior<,>));
    List<string> postprocessors = GetComponentOrder(serviceCollection, typeof(IRequestPostProcessor<,>));

    var message = new StringBuilder("TimeWarp State (MediatR) Middleware Registrations:");
    message.AppendLine();
    message.AppendLine();
    
    AppendComponentOrder(message, "Preprocessors", preprocessors);
    AppendComponentOrder(message, "Behaviors", middleware);
    AppendComponentOrder(message, "Postprocessors", postprocessors);

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
