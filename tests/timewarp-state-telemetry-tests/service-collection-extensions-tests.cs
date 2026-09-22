#region Purpose
// Proves AddTimeWarpStateTelemetry uses TryAdd so a second call does not replace registrations.
#endregion

namespace ServiceCollectionExtensionsTests;

public class Should_
{
  public void Register_Options_And_Snapshot_Cache_Once()
  {
    ServiceCollection serviceCollection = new();

    serviceCollection.AddTimeWarpStateTelemetry(options => options.IncludeSnapshots = true);
    serviceCollection.AddTimeWarpStateTelemetry(options => options.IncludeSnapshots = false);

    serviceCollection.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(TimeWarpStateTelemetryOptions))
      .ShouldBe(1);
    serviceCollection.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(StateSnapshotCache))
      .ShouldBe(1);

    ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
    TimeWarpStateTelemetryOptions timeWarpStateTelemetryOptions =
      serviceProvider.GetRequiredService<TimeWarpStateTelemetryOptions>();
    timeWarpStateTelemetryOptions.IncludeSnapshots.ShouldBeTrue();

    using IServiceScope serviceScope = serviceProvider.CreateScope();
    serviceScope.ServiceProvider.GetRequiredService<StateSnapshotCache>().ShouldNotBeNull();
  }

  public void Expose_Activity_Source_Name_For_AddSource()
  {
    TimeWarpStateTelemetry.ActivitySourceName.ShouldBe("TimeWarp.State");
    TimeWarpStateTelemetry.ActivitySource.Name.ShouldBe("TimeWarp.State");
  }
}
