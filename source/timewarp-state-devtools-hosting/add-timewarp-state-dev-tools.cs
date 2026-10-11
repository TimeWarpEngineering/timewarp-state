#region Purpose
// Adds the DevTools Blazor app to an Aspire AppHost and points an inspected app at it.
#endregion

#region Design
// AddProject by path, so the AppHost does not need a compile-time Projects.* type for DevTools.
// WithHttpEndpoint(name: "http") declares the UI endpoint. Aspire updates an endpoint of the same
// name, so a launch profile that already added "http" is not duplicated. Without a profile, that
// call is what gives the resource an endpoint and lets Aspire set ASPNETCORE_URLS. The dashboard
// text is "TimeWarp.State DevTools". WithTimeWarpStateDevTools sets
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT from that endpoint and waits until DevTools is running.
// The OTLP endpoint Aspire already injects is left alone; the dashboard still receives traces.
#endregion

namespace TimeWarp.State.DevTools.Hosting;

/// <summary>
/// Names used by <see cref="TimeWarpStateDevToolsHostingExtensions"/>.
/// </summary>
public static class TimeWarpStateDevToolsHosting
{
  /// <summary>
  /// Environment variable that carries the DevTools origin to the inspected app. Must equal
  /// <c>TimeWarpStateDevToolsExport.EndpointEnvironmentVariable</c>; a test asserts the match.
  /// </summary>
  public const string EndpointEnvironmentVariable = "TIMEWARP_STATE_DEVTOOLS_ENDPOINT";

  /// <summary>Default Aspire resource name.</summary>
  public const string DefaultResourceName = "timewarp-state-devtools";

  /// <summary>
  /// HTTP endpoint name. <see cref="TimeWarpStateDevToolsHostingExtensions.AddTimeWarpStateDevTools"/>
  /// declares it with <c>WithHttpEndpoint</c>. The DevTools launch profile uses the same name.
  /// </summary>
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
  /// Adds the DevTools Blazor app as a project resource, declares its HTTP endpoint, and labels that
  /// endpoint on the dashboard.
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
      .WithHttpEndpoint(name: TimeWarpStateDevToolsHosting.UiEndpointName)
      .WithUrlForEndpoint
      (
        TimeWarpStateDevToolsHosting.UiEndpointName,
        url => url.DisplayText = TimeWarpStateDevToolsHosting.DashboardLinkText
      );
  }

  /// <summary>
  /// Publishes the DevTools origin to <paramref name="builder"/> and waits for DevTools to start.
  /// The inspected app reads <see cref="TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable"/>
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
        TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable,
        devtools.GetEndpoint(TimeWarpStateDevToolsHosting.UiEndpointName)
      )
      .WaitFor(devtools);
  }
}
