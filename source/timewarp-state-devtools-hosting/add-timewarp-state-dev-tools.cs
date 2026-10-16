#region Purpose
// Adds the packaged DevTools inspector to an Aspire AppHost and points an inspected app at it.
#endregion

#region Design
// TimeWarp.State.DevTools.App is a framework-dependent package with no runtime identifier.
// Its tools/ folder is the publish output. Package targets stamp AssemblyMetadata
// timewarpstatedevtoolspath onto the AppHost. AddExecutable starts that dll with the .NET host:
// DOTNET_HOST_PATH, otherwise the running muxer when this process is dotnet or dotnet.exe,
// otherwise DOTNET_ROOT/dotnet (dotnet.exe on Windows), otherwise the name "dotnet".
// An apphost or IDE launch does not put that muxer on PATH. Per-RID packages were rejected.
// ASPNETCORE_HTTP_PORTS receives the http endpoint port. The endpoint name stays http.
// The packaged tools/ folder has no launch profile, so the executable would otherwise start as
// Production and ingest CORS would stay off. In run mode the executable gets
// ASPNETCORE_ENVIRONMENT=Development. Publish mode does not set that variable.
// WithTimeWarpStateDevTools sets TIMEWARP_STATE_DEVTOOLS_ENDPOINT and waits for the inspector
// in run mode. It also adds the inspected resource's http and https endpoints to one
// TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS callback, so a later inspected app does not replace
// the first app's origins. In publish mode it returns the inspected resource unchanged.
// ExcludeFromManifest keeps the inspector resource itself out of the manifest, so publish
// output has no reference to it.
// A missing inspector file is an error in run mode and is ignored in publish mode.
// IResourceBuilder<T> is invariant, so WaitFor rewraps the inspector as IResource.
// AddTimeWarpStateDevToolsProject is the inner-loop switch: the caller passes a csproj path.
// There is no parent-folder walk and no TIMEWARP_STATE_DEVTOOLS_PROJECT override.
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
  public const string DefaultResourceName = "timewarp-devtools";

  /// <summary>
  /// HTTP endpoint name. <see cref="TimeWarpStateDevToolsHostingExtensions.AddTimeWarpStateDevTools"/>
  /// declares it with <c>WithHttpEndpoint</c>.
  /// </summary>
  public const string UiEndpointName = "http";

  /// <summary>Dashboard link text for the DevTools UI.</summary>
  public const string DashboardLinkText = "TimeWarp DevTools";

  /// <summary>
  /// Assembly metadata key whose value is the full path of <c>TimeWarp.State.DevTools.dll</c>
  /// inside the restored <c>TimeWarp.State.DevTools.App</c> package.
  /// </summary>
  public const string AppPathMetadataKey = "timewarpstatedevtoolspath";

  /// <summary>File name of the published inspector assembly.</summary>
  public const string AppAssemblyFileName = "TimeWarp.State.DevTools.dll";

  /// <summary>
  /// Fallback command name when <see cref="ResolveAppLaunchCommand()"/> finds no host path.
  /// </summary>
  public const string AppLaunchCommand = "dotnet";

  /// <summary>Environment variable that receives the inspector's HTTP target port.</summary>
  public const string HttpPortsEnvironmentVariable = "ASPNETCORE_HTTP_PORTS";

  /// <summary>
  /// Environment variable set to <see cref="DevelopmentEnvironmentName"/> on the packaged
  /// inspector in run mode. The published tools folder has no launch profile.
  /// </summary>
  public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";

  /// <summary>Value of <see cref="AspNetCoreEnvironmentVariable"/> in run mode.</summary>
  public const string DevelopmentEnvironmentName = "Development";

  /// <summary>
  /// Comma-separated browser origins allowed to call the inspector ingest route.
  /// Must equal <c>DevToolsIngestCors.OriginsEnvironmentVariable</c>. A test asserts the match.
  /// </summary>
  public const string CorsOriginsEnvironmentVariable = "TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS";

  /// <summary>
  /// .NET host Aspire should execute. <see cref="AppLaunchCommand"/> is the last fallback.
  /// </summary>
  internal static string ResolveAppLaunchCommand()
  {
    return ResolveAppLaunchCommand
    (
      Environment.GetEnvironmentVariable("DOTNET_HOST_PATH"),
      Environment.ProcessPath,
      Environment.GetEnvironmentVariable("DOTNET_ROOT")
    );
  }

  /// <summary>
  /// Resolves the host from an explicit <paramref name="dotnetHostPath"/>, then
  /// <paramref name="processPath"/> when that file is the muxer, then <paramref name="dotnetRoot"/>.
  /// </summary>
  internal static string ResolveAppLaunchCommand
  (
    string? dotnetHostPath,
    string? processPath,
    string? dotnetRoot
  )
  {
    if (!string.IsNullOrWhiteSpace(dotnetHostPath))
    {
      return dotnetHostPath.Trim();
    }

    if (IsDotNetMuxer(processPath))
    {
      return processPath!;
    }

    if (!string.IsNullOrWhiteSpace(dotnetRoot))
    {
      string hostFileName = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
      return Path.Combine(dotnetRoot.Trim(), hostFileName);
    }

    return AppLaunchCommand;
  }

  private static bool IsDotNetMuxer(string? processPath)
  {
    if (string.IsNullOrWhiteSpace(processPath))
    {
      return false;
    }

    string fileName = Path.GetFileName(processPath);
    return fileName.Equals("dotnet", StringComparison.OrdinalIgnoreCase)
      || fileName.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase);
  }
}

/// <summary>
/// Aspire registration for the TimeWarp DevTools app.
/// </summary>
public static class TimeWarpStateDevToolsHostingExtensions
{
  /// <summary>
  /// Adds the packaged DevTools inspector as an executable resource, declares its HTTP endpoint,
  /// and labels that endpoint on the dashboard. In run mode the executable's
  /// <see cref="TimeWarpStateDevToolsHosting.AspNetCoreEnvironmentVariable"/> is
  /// <see cref="TimeWarpStateDevToolsHosting.DevelopmentEnvironmentName"/>. The resource is
  /// omitted from publish output.
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
    bool publishMode = builder.ExecutionContext.IsPublishMode;
    if (string.IsNullOrWhiteSpace(appPath) || (!publishMode && !File.Exists(appPath)))
    {
      throw new InvalidOperationException
      (
        "TimeWarp DevTools inspector path is not set. Reference TimeWarp.State.DevTools.Hosting " +
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

    IResourceBuilder<ExecutableResource> executable = builder
      .AddExecutable
      (
        name,
        TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand(),
        appDirectory,
        appPath
      )
      .WithHttpEndpoint
      (
        name: TimeWarpStateDevToolsHosting.UiEndpointName,
        env: TimeWarpStateDevToolsHosting.HttpPortsEnvironmentVariable
      )
      .LabelDevToolsUi();

    if (!publishMode)
    {
      executable.WithEnvironment
      (
        TimeWarpStateDevToolsHosting.AspNetCoreEnvironmentVariable,
        TimeWarpStateDevToolsHosting.DevelopmentEnvironmentName
      );
    }

    return executable;
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
  /// and calls <c>AddTimeWarpStateDevToolsExporter</c>. The inspector receives
  /// <see cref="TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable"/> from the inspected
  /// resource's http and https endpoints. Each call adds to the same origin list.
  /// In publish mode the inspected resource is returned unchanged, so publish output
  /// does not reference the inspector.
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
    where TDevTools : IResourceWithEndpoints, IResourceWithEnvironment
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(devtools);

    if (builder.ApplicationBuilder.ExecutionContext.IsPublishMode)
    {
      return builder;
    }

    PublishClientOrigins(builder, devtools);

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

  private static void PublishClientOrigins<T, TDevTools>
  (
    IResourceBuilder<T> inspected,
    IResourceBuilder<TDevTools> devtools
  )
    where T : IResource
    where TDevTools : IResourceWithEnvironment
  {
    if (inspected.Resource is not IResourceWithEndpoints client)
    {
      return;
    }

    List<EndpointReference> origins = [];
    EndpointReference https = new(client, "https");
    EndpointReference http = new(client, "http");
    if (https.Exists)
    {
      origins.Add(https);
    }

    if (http.Exists)
    {
      origins.Add(http);
    }

    if (origins.Count == 0)
    {
      return;
    }

    CorsOriginsAnnotation annotation = devtools.Resource.Annotations
      .OfType<CorsOriginsAnnotation>()
      .SingleOrDefault() ?? RegisterCorsOrigins(devtools);
    annotation.Endpoints.AddRange(origins);
  }

  private static CorsOriginsAnnotation RegisterCorsOrigins<TDevTools>(IResourceBuilder<TDevTools> devtools)
    where TDevTools : IResourceWithEnvironment
  {
    CorsOriginsAnnotation annotation = new();
    devtools.WithAnnotation(annotation);
    devtools.WithEnvironment(context =>
    {
      CorsOriginsAnnotation collected = context.Resource.Annotations
        .OfType<CorsOriginsAnnotation>()
        .Single();
      if (collected.Endpoints.Count == 0)
      {
        return;
      }

      ReferenceExpressionBuilder expression = new();
      for (int index = 0; index < collected.Endpoints.Count; index++)
      {
        if (index > 0)
        {
          expression.AppendLiteral(",");
        }

        expression.AppendValueProvider(collected.Endpoints[index]);
      }

      context.EnvironmentVariables[TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable] = expression.Build();
    });
    return annotation;
  }

  private sealed class CorsOriginsAnnotation : IResourceAnnotation
  {
    public List<EndpointReference> Endpoints { get; } = [];
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
