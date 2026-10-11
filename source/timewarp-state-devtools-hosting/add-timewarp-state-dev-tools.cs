#region Purpose
// Adds the DevTools Blazor app to an Aspire AppHost and points an inspected app at it.
#endregion

#region Design
// AddProject by path, so the AppHost does not need a compile-time Projects.* type for DevTools.
// The http endpoint's dashboard text is "TimeWarp.State DevTools". WithTimeWarpStateDevTools sets
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT from that endpoint and waits until DevTools is running.
// The OTLP endpoint Aspire already injects is left alone; the dashboard still receives traces.
#endregion

namespace TimeWarp.State.DevTools.Hosting;

/// <summary>
/// Names used by <see cref="TimeWarpStateDevToolsHostingExtensions"/>.
/// </summary>
public static class TimeWarpStateDevToolsHosting
{
  /// <summary>Default Aspire resource name.</summary>
  public const string DefaultResourceName = "timewarp-state-devtools";

  /// <summary>HTTP endpoint name taken from the DevTools launch profile.</summary>
  public const string UiEndpointName = "http";

  /// <summary>Dashboard link text for the DevTools UI.</summary>
  public const string DashboardLinkText = "TimeWarp.State DevTools";
}

/// <summary>
/// Aspire registration for the TimeWarp.State DevTools app.
/// </summary>
public static class TimeWarpStateDevToolsHostingExtensions
{
  /// <summary>
  /// Adds the DevTools Blazor app as a project resource and labels its HTTP endpoint on the dashboard.
  /// </summary>
  /// <param name="builder">The distributed application builder.</param>
  /// <param name="name">Resource name. Defaults to <see cref="TimeWarpStateDevToolsHosting.DefaultResourceName"/>.</param>
  /// <returns>The DevTools project resource.</returns>
  public static IResourceBuilder<ProjectResource> AddTimeWarpStateDevTools
  (
    this IDistributedApplicationBuilder builder,
    string name = TimeWarpStateDevToolsHosting.DefaultResourceName
  )
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    string projectPath = DevToolsProjectLocator.Resolve(builder.AppHostDirectory);
    return builder
      .AddProject(name, projectPath)
      .WithUrlForEndpoint
      (
        TimeWarpStateDevToolsHosting.UiEndpointName,
        url => url.DisplayText = TimeWarpStateDevToolsHosting.DashboardLinkText
      );
  }

  /// <summary>
  /// Publishes the DevTools origin to <paramref name="builder"/> and waits for DevTools to start.
  /// The inspected app reads <see cref="TimeWarpStateDevToolsExport.EndpointEnvironmentVariable"/>
  /// and calls <c>AddTimeWarpStateDevToolsExporter</c>.
  /// </summary>
  /// <typeparam name="T">Inspected resource type.</typeparam>
  /// <param name="builder">The inspected resource.</param>
  /// <param name="devtools">The resource from <see cref="AddTimeWarpStateDevTools"/>.</param>
  /// <returns>The inspected resource builder.</returns>
  public static IResourceBuilder<T> WithTimeWarpStateDevTools<T>
  (
    this IResourceBuilder<T> builder,
    IResourceBuilder<ProjectResource> devtools
  )
    where T : IResourceWithEnvironment, IResourceWithWaitSupport
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(devtools);

    return builder
      .WithEnvironment
      (
        TimeWarpStateDevToolsExport.EndpointEnvironmentVariable,
        devtools.GetEndpoint(TimeWarpStateDevToolsHosting.UiEndpointName)
      )
      .WaitFor(devtools);
  }
}
