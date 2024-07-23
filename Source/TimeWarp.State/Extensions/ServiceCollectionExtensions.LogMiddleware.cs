namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  public static void LogMiddleware(this IServiceCollection serviceCollection, ILogger logger)
  {
    List<string> order = GetMiddlewareOrder(serviceCollection);
    LogOrder(logger, order);
  }

  public static List<string> GetMiddlewareOrder(this IServiceCollection serviceCollection)
  {
    return serviceCollection
      .Where(sd => sd.ServiceType.IsGenericType && 
        sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
      .Select(sd => sd.ImplementationType?.Name ?? "Unknown")
      .ToList();
  }

  private static void LogOrder(ILogger logger, IReadOnlyList<string> order)
  {
    var message = new StringBuilder("MediatR middleware registration order:\n");
    for (int i = 0; i < order.Count; i++)
    {
      message.AppendLine($"{i + 1}. {order[i]}");
    }
    logger.LogInformation(message.ToString());
  }
}
