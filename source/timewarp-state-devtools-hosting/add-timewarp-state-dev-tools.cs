#region Purpose
// Adds the packaged DevTools inspector to an Aspire AppHost and points an inspected app at it.
#endregion

#region Design
// TimeWarp.State.DevTools.App is a framework-dependent NuGet package with no runtime identifier.
// Its tools/ folder is the publish output. The package targets stamp AssemblyMetadata
// timewarpstatedevtoolspath onto the Aspire AppHost (and onto test assemblies that opt in).
// AddExecutable runs `dotnet` on that dll, which is already on PATH because the AppHost is a
// dotnet process. Per-RID self-contained packages were rejected: the dll is portable.
// ASPNETCORE_HTTP_PORTS receives the http endpoint's target port. The endpoint name stays http
// and the dashboard text stays "TimeWarp.State DevTools". WithTimeWarpStateDevTools sets
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT from that endpoint and waits until the inspector is running.
// IResourceBuilder<T> is invariant, so WaitFor cannot take IResourceBuilder<TDevTools>. The call
// rewraps devtools.Resource as IResourceBuilder<IResource> and uses Aspire's WaitFor.
// ExcludeFromManifest keeps the inspector out of publish output.
// AddTimeWarpStateDevToolsProject is the inner-loop switch: the caller passes a csproj path.
// There is no parent-folder walk and no TIMEWARP_STATE_DEVTOOLS_PROJECT override.
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
  /// declares it with <c>WithHttpEndpoint</c>.
  /// </summary>
  public const string UiEndpointName = "http";

  /// <summary>Dashboard link text for the DevTools UI.</summary>
  public const string DashboardLinkText = "TimeWarp.State DevTools";

  /// <summary>
  /// Assembly metadata key whose value is the full path of <c>TimeWarp.State.DevTools.dll</c>
  /// inside the restored <c>TimeWarp.State.DevTools.App</c> package.
  /// </summary>
  public const string AppPathMetadataKey = "timewarpstatedevtoolspath";

  /// <summary>File name of the published inspector assembly.</summary>
  public const string AppAssemblyFileName = "TimeWarp.State.DevTools.dll";

  /// <summary>Command Aspire runs. Resolved from the AppHost PATH.</summary>
  public const string AppLaunchCommand = "dotnet";

  /// <summary>Environment variable that receives the inspector's HTTP target port.</summary>
  public const string HttpPortsEnvironmentVariable = "ASPNETCORE_HTTP_PORTS";
}

/// <summary>
/// Aspire registration for the TimeWarp.State DevTools app.
/// </summary>
public static class TimeWarpStateDevToolsHostingExtensions
{
  /// <summary>
  /// Adds the packaged DevTools inspector as an executable resource, declares its HTTP endpoint,
  /// and labels that endpoint on the dashboard. The resource is omitted from publish output.
  /// </summary>
  /// <param name="builder">The distributed application builder.</param>
  /// <param name="name">Resource name. Defaults to <see cref="TimeWarpStateDevToolsHosting.DefaultResourceName"/>.</param>
  /// <returns>The DevTools executable resource.</returns>
  /// <exception cref="InvalidOperationException">The package targets did not stamp the inspector path.</exception>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static IResourceBuilder<ExecutableResource> AddTimeWarpStateDevTools
  (
    this IDistributedApplicationBuilder builder,
    string name = TimeWarpStateDevToolsHosting.DefaultResourceName
  )
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    string? appPath = ReadAppPath(Assembly.GetEntryAssembly())
      ?? ReadAppPath(Assembly.GetCallingAssembly());
    if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
    {
      throw new InvalidOperationException
      (
        "TimeWarp.State DevTools inspector path is not set. Reference TimeWarp.State.DevTools.Hosting " +
        "from an Aspire AppHost so TimeWarp.State.DevTools.App can stamp " +
        $"{TimeWarpStateDevToolsHosting.AppPathMetadataKey}. For inner-loop work on the inspector, " +
        "call AddTimeWarpStateDevToolsProject with the csproj path." +
        (string.IsNullOrWhiteSpace(appPath) ? "" : $" Missing file: '{appPath}'.")
      );
    }

    string? appDirectory = Path.GetDirectoryName(appPath);
    if (string.IsNullOrWhiteSpace(appDirectory))
    {
      throw new InvalidOperationException($"Inspector path has no directory: '{appPath}'.");
    }

    return builder
      .AddExecutable
      (
        name,
        TimeWarpStateDevToolsHosting.AppLaunchCommand,
        appDirectory,
        appPath
      )
      .WithHttpEndpoint
      (
        name: TimeWarpStateDevToolsHosting.UiEndpointName,
        env: TimeWarpStateDevToolsHosting.HttpPortsEnvironmentVariable
      )
      .LabelDevToolsUi();
  }

  /// <summary>
  /// Adds the DevTools inspector project at <paramref name="projectPath"/>. Inner-loop switch for
  /// work on the inspector. The path is explicit. Nothing walks parent folders.
  /// </summary>
  /// <param name="builder">The distributed application builder.</param>
  /// <param name="projectPath">Full or relative path of the inspector csproj.</param>
  /// <param name="name">Resource name. Defaults to <see cref="TimeWarpStateDevToolsHosting.DefaultResourceName"/>.</param>
  /// <returns>The DevTools project resource.</returns>
  /// <exception cref="InvalidOperationException"><paramref name="projectPath"/> is not a file.</exception>
  public static IResourceBuilder<ProjectResource> AddTimeWarpStateDevToolsProject
  (
    this IDistributedApplicationBuilder builder,
    string projectPath,
    string name = TimeWarpStateDevToolsHosting.DefaultResourceName
  )
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

    string fullPath = Path.GetFullPath(projectPath);
    if (!File.Exists(fullPath))
    {
      throw new InvalidOperationException($"DevTools project does not exist: '{fullPath}'.");
    }

    return builder
      .AddProject(name, fullPath)
      .WithHttpEndpoint(name: TimeWarpStateDevToolsHosting.UiEndpointName)
      .LabelDevToolsUi();
  }

  /// <summary>
  /// Publishes the DevTools origin to <paramref name="builder"/> and waits for DevTools to start.
  /// The inspected app reads <see cref="TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable"/>
  /// and calls <c>AddTimeWarpStateDevToolsExporter</c>.
  /// </summary>
  /// <typeparam name="T">Inspected resource type.</typeparam>
  /// <typeparam name="TDevTools">DevTools resource type. The packaged app is an executable; the project switch is a project.</typeparam>
  /// <param name="builder">The inspected resource.</param>
  /// <param name="devtools">The resource from <see cref="AddTimeWarpStateDevTools"/> or <see cref="AddTimeWarpStateDevToolsProject"/>.</param>
  /// <returns>The inspected resource builder.</returns>
  public static IResourceBuilder<T> WithTimeWarpStateDevTools<T, TDevTools>
  (
    this IResourceBuilder<T> builder,
    IResourceBuilder<TDevTools> devtools
  )
    where T : IResourceWithEnvironment, IResourceWithWaitSupport
    where TDevTools : IResourceWithEndpoints
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(devtools);

    IResource devtoolsResource = devtools.Resource;
    IResourceBuilder<IResource> dependency = builder.ApplicationBuilder.CreateResourceBuilder(devtoolsResource);

    return builder
      .WithEnvironment
      (
        TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable,
        devtools.GetEndpoint(TimeWarpStateDevToolsHosting.UiEndpointName)
      )
      .WaitFor(dependency);
  }

  private static IResourceBuilder<T> LabelDevToolsUi<T>(this IResourceBuilder<T> builder)
    where T : IResource
  {
    return builder
      .WithUrlForEndpoint
      (
        TimeWarpStateDevToolsHosting.UiEndpointName,
        url => url.DisplayText = TimeWarpStateDevToolsHosting.DashboardLinkText
      )
      .ExcludeFromManifest();
  }

  private static string? ReadAppPath(Assembly? assembly)
  {
    if (assembly is null)
    {
      return null;
    }

    foreach (AssemblyMetadataAttribute attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
    {
      if (string.Equals(attribute.Key, TimeWarpStateDevToolsHosting.AppPathMetadataKey, StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(attribute.Value))
      {
        return attribute.Value;
      }
    }

    return null;
  }
}
