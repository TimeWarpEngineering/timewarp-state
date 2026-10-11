#region Purpose
// Host entry for the inspected-app fixture: wires the store and the DevTools exporter reference.
#endregion

#region Design
// Project references replace the sample's PackageReferences so this assembly restores before pack. Calling AddGeneratedMediator emits the sender into this assembly, which is what the isolation test reads. ExporterAssembly names the exporter marker from a property body so the compiler keeps the TimeWarp.State.DevTools.Export reference; a discarded typeof or a const read is removed. This project does not reference the DevTools app.
#endregion

namespace InspectedApp;

/// <summary>
/// Entry point for the inspected-app fixture.
/// </summary>
public static class Program
{
  /// <summary>
  /// Registers the store pipeline.
  /// </summary>
  public static void Main()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator<ClientPipeline>();
    services.AddTimeWarpState();
  }

  /// <summary>
  /// Assembly name of the DevTools exporter this host is wired to.
  /// </summary>
  public static string ExporterAssembly =>
    typeof(TimeWarp.State.DevTools.Export.AssemblyMarker).Assembly.GetName().Name ?? string.Empty;
}
