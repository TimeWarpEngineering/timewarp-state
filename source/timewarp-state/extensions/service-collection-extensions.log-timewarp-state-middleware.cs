#region Purpose
// Logs kind, scope, and pipeline-order behaviors from a host MediatorManifest JSON document.
#endregion

#region Design
// 15.0 weaves behaviors and does not register IPipelineBehavior<,>, so the service collection is not
// the pipeline. The host passes MediatorManifest.Json. GetComponentOrder keeps its signature and
// returns the distinct simple names from the last Read when the caller asks for IPipelineBehavior<,>.
// Any other component type returns an empty list. The service collection argument is validated and
// is not scanned.
#endregion

namespace TimeWarp.State;

public static partial class ServiceCollectionExtensions
{
  /// <summary>
  /// Logs the requests and behaviors in <paramref name="mediatorManifestJson"/>.
  /// </summary>
  /// <param name="serviceCollection">The host service collection. It is not scanned.</param>
  /// <param name="logger">Logger that receives the pipeline report.</param>
  /// <param name="mediatorManifestJson"><c>MediatorManifest.Json</c> from this host.</param>
  public static void LogTimeWarpStateMiddleware
  (
    this IServiceCollection serviceCollection,
    ILogger logger,
    string mediatorManifestJson
  )
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);
    ArgumentNullException.ThrowIfNull(logger);
    PipelineBehaviorRegistry.Read(mediatorManifestJson);

    StringBuilder message = new("TimeWarp State (TimeWarp.Mediator) Pipeline Behavior Registrations:");
    message.AppendLine();
    message.AppendLine();
    message.AppendLine("MediatorManifest version 2");
    message.AppendLine();

    IReadOnlyList<PipelineBehaviorRegistry.ManifestRequest> requests = PipelineBehaviorRegistry.Requests;
    message.AppendLine("Requests:");
    for (int i = 0; i < requests.Count; i++)
    {
      PipelineBehaviorRegistry.ManifestRequest request = requests[i];
      string scope = request.Scope is null ? "unscoped" : request.Scope;
      message.AppendLine($"  {i + 1}. {request.Request} (kind {request.Kind}, scope {scope})");
    }

    message.AppendLine();
    AppendComponentOrder(message, "Behaviors (in pipeline order)", PipelineBehaviorRegistry.BehaviorNames());
    logger.LogInformation(message.ToString());
  }

  /// <summary>
  /// Lists distinct behavior simple names from the manifest last read into <see cref="PipelineBehaviorRegistry"/>.
  /// </summary>
  /// <remarks>
  /// Only <c>IPipelineBehavior&lt;,&gt;</c> returns names. The list is manifest order, not service-collection order.
  /// A call before <see cref="LogTimeWarpStateMiddleware"/> or <see cref="PipelineBehaviorRegistry.Read"/> returns an empty list.
  /// </remarks>
  public static List<string> GetComponentOrder(this IServiceCollection serviceCollection, Type componentType)
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);
    ArgumentNullException.ThrowIfNull(componentType);
    if (componentType != typeof(IPipelineBehavior<,>))
    {
      return [];
    }

    return PipelineBehaviorRegistry.BehaviorNames().ToList();
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
