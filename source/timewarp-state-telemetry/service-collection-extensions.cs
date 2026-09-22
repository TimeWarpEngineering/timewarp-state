#region Purpose
// Registers telemetry options and the per-scope snapshot cache with TryAdd.
#endregion

namespace TimeWarp.State.Telemetry;

/// <summary>
/// DI registration for TimeWarp.State.Telemetry.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers <see cref="TimeWarpStateTelemetryOptions"/> and <see cref="StateSnapshotCache"/>.
  /// Uses <c>TryAdd*</c> so a host can replace either registration.
  /// </summary>
  /// <param name="serviceCollection">The service collection.</param>
  /// <param name="configure">Optional snapshot and serializer configuration.</param>
  /// <returns>The same service collection.</returns>
  public static IServiceCollection AddTimeWarpStateTelemetry
  (
    this IServiceCollection serviceCollection,
    Action<TimeWarpStateTelemetryOptions>? configure = null
  )
  {
    ArgumentNullException.ThrowIfNull(serviceCollection);

    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions = new();
    configure?.Invoke(timeWarpStateTelemetryOptions);
    serviceCollection.TryAddSingleton(timeWarpStateTelemetryOptions);
    serviceCollection.TryAddScoped<StateSnapshotCache>();
    return serviceCollection;
  }
}
