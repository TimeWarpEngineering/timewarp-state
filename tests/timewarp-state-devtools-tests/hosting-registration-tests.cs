#region Purpose
// Proves AddTimeWarpStateDevTools starts the packaged inspector and labels the dashboard link.
#endregion

#region Design
// The builder is not started. The test assembly opts into the package targets, so the stamped
// path is the in-repo publish output under timewarp-state-devtools-app/tools. Nothing asserts a
// csproj path. URL and environment callbacks run against a seeded http endpoint so the dashboard
// text and TIMEWARP_STATE_DEVTOOLS_ENDPOINT are visible without the Aspire dashboard.
// AddTimeWarpStateDevToolsProject covers the explicit csproj switch.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class AddTimeWarpStateDevTools_Should
{
  public static void RunThePackagedAssembly()
  {
    string path = StampedAppPath();
    Path.GetFileName(path).ShouldBe(TimeWarpStateDevToolsHosting.AppAssemblyFileName);
    path.Contains("timewarp-state-devtools-app", StringComparison.Ordinal).ShouldBeTrue();
    path.Contains($"{Path.DirectorySeparatorChar}tools{Path.DirectorySeparatorChar}", StringComparison.Ordinal).ShouldBeTrue();
    path.Contains("timewarp-state-devtools.csproj", StringComparison.Ordinal).ShouldBeFalse();
    File.Exists(path).ShouldBeTrue();

    IDistributedApplicationBuilder builder = CreateBuilder();
    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    devtools.Resource.Name.ShouldBe(TimeWarpStateDevToolsHosting.DefaultResourceName);
    devtools.Resource.Command.ShouldBe(TimeWarpStateDevToolsHosting.AppLaunchCommand);
    devtools.Resource.WorkingDirectory.ShouldBe(Path.GetDirectoryName(path));
    devtools.Resource.Annotations.OfType<IProjectMetadata>().ShouldBeEmpty();
    devtools.Resource.IsExcludedFromPublish().ShouldBeTrue();

    EndpointAnnotation httpEndpoint = HttpEndpoint(devtools.Resource);
    httpEndpoint.UriScheme.ShouldBe("http");
    PropertyInfo? portVariable = typeof(EndpointAnnotation).GetProperty
    (
      "TargetPortEnvironmentVariable",
      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );
    portVariable.ShouldNotBeNull();
    portVariable.GetValue(httpEndpoint).ShouldBe(TimeWarpStateDevToolsHosting.HttpPortsEnvironmentVariable);
  }

  public static async Task PassTheAssemblyPathToDotnet()
  {
    string path = StampedAppPath();
    IDistributedApplicationBuilder builder = CreateBuilder();
    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    List<object> args = await ApplyArgumentCallbacks(devtools.Resource);
    args.OfType<string>().ShouldContain(path);
  }

  public static void IgnoreTheRetiredProjectPathVariable()
  {
    string? previous = Environment.GetEnvironmentVariable("TIMEWARP_STATE_DEVTOOLS_PROJECT");
    Environment.SetEnvironmentVariable("TIMEWARP_STATE_DEVTOOLS_PROJECT", "/tmp/does-not-exist.csproj");
    try
    {
      IDistributedApplicationBuilder builder = CreateBuilder();
      IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
      devtools.Resource.Command.ShouldBe(TimeWarpStateDevToolsHosting.AppLaunchCommand);
      devtools.Resource.Annotations.OfType<IProjectMetadata>().ShouldBeEmpty();
    }
    finally
    {
      Environment.SetEnvironmentVariable("TIMEWARP_STATE_DEVTOOLS_PROJECT", previous);
    }
  }

  public static void NotExposeASourcePathLocator()
  {
    typeof(TimeWarpStateDevToolsHosting).Assembly
      .GetType("TimeWarp.State.DevTools.Hosting.DevToolsProjectLocator")
      .ShouldBeNull();
  }

  public static void AddTheInspectorProjectWhenThePathIsExplicit()
  {
    string project = Path.Combine(RepoPaths.Root(), "source", "timewarp-state-devtools", "timewarp-state-devtools.csproj");
    IDistributedApplicationBuilder builder = CreateBuilder();
    IResourceBuilder<ProjectResource> devtools = builder.AddTimeWarpStateDevToolsProject(project);
    devtools.Resource.Annotations.OfType<IProjectMetadata>().Single().ProjectPath.ShouldBe(Path.GetFullPath(project));
    HttpEndpoint(devtools.Resource).UriScheme.ShouldBe("http");
    devtools.Resource.IsExcludedFromPublish().ShouldBeTrue();
    devtools.Resource.Name.ShouldBe(TimeWarpStateDevToolsHosting.DefaultResourceName);
  }

  public static void RejectAMissingExplicitProjectPath()
  {
    IDistributedApplicationBuilder builder = CreateBuilder();
    Should.Throw<InvalidOperationException>
    (
      () => builder.AddTimeWarpStateDevToolsProject(Path.Combine(Path.GetTempPath(), "missing-devtools.csproj"))
    );
  }

  public static async Task LabelTheDashboardLinkAndPublishTheEndpoint()
  {
    string root = RepoPaths.Root();
    IDistributedApplicationBuilder builder = CreateBuilder();

    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    EndpointAnnotation httpEndpoint = HttpEndpoint(devtools.Resource);
    httpEndpoint.UriScheme.ShouldBe("http");
    httpEndpoint.Name.ShouldBe(TimeWarpStateDevToolsHosting.UiEndpointName);

    string serverProject = Path.Combine(root, "samples/04-telemetry/server/sample-04-server/sample-04-server.csproj");
    IResourceBuilder<ProjectResource> server = builder
      .AddProject("sample-04-server", serverProject)
      .WithTimeWarpStateDevTools(devtools);

    server.Resource.Annotations.OfType<WaitAnnotation>()
      .ShouldContain(annotation => annotation.Resource == devtools.Resource);

    (await DashboardLabel(devtools.Resource)).ShouldBe(TimeWarpStateDevToolsHosting.DashboardLinkText);

    Dictionary<string, object> variables = await ApplyEnvironmentCallbacks(server.Resource);
    variables.ContainsKey(TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable).ShouldBeTrue();
  }

  public static void ShareTheEndpointVariableNameWithTheExporter()
  {
    TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable
      .ShouldBe(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
  }

  private static string StampedAppPath()
  {
    string? path = typeof(AddTimeWarpStateDevTools_Should).Assembly
      .GetCustomAttributes<AssemblyMetadataAttribute>()
      .FirstOrDefault(attribute => attribute.Key == TimeWarpStateDevToolsHosting.AppPathMetadataKey)
      ?.Value;
    path.ShouldNotBeNullOrWhiteSpace();
    return path;
  }

  private static IDistributedApplicationBuilder CreateBuilder()
  {
    return DistributedApplication.CreateBuilder
    (
      new DistributedApplicationOptions
      {
        Args = [],
        DisableDashboard = true,
        AllowUnsecuredTransport = true,
        AssemblyName = "TimeWarp.State.DevTools.Tests",
        ProjectDirectory = Path.Combine(RepoPaths.Root(), "samples/04-telemetry/apphost")
      }
    );
  }

  private static EndpointAnnotation HttpEndpoint(IResource resource)
  {
    return resource.Annotations
      .OfType<EndpointAnnotation>()
      .Where
      (
        endpoint => string.Equals
        (
          endpoint.Name,
          TimeWarpStateDevToolsHosting.UiEndpointName,
          StringComparison.OrdinalIgnoreCase
        )
      )
      .ShouldHaveSingleItem();
  }

  private static async Task<string?> DashboardLabel(IResource resource)
  {
    DistributedApplicationExecutionContext execution = new(DistributedApplicationOperation.Run);
    EndpointAnnotation endpoint = new(ProtocolType.Tcp, "http", "http", "http");
    EndpointReference reference = new((IResourceWithEndpoints)resource, endpoint);
    string? label = null;
    foreach (ResourceUrlsCallbackAnnotation annotation in resource.Annotations.OfType<ResourceUrlsCallbackAnnotation>())
    {
      ResourceUrlAnnotation url = new()
      {
        Endpoint = reference,
        Url = "http://localhost:5288"
      };
      ResourceUrlsCallbackContext context = new(execution, resource, [url], CancellationToken.None)
      {
        Logger = NullLogger.Instance
      };
      await annotation.Callback(context);
      ResourceUrlAnnotation? labeled = context.Urls.FirstOrDefault(candidate =>
        candidate.DisplayText == TimeWarpStateDevToolsHosting.DashboardLinkText);
      if (labeled is not null)
      {
        label = labeled.DisplayText;
      }
    }

    return label;
  }

  private static async Task<Dictionary<string, object>> ApplyEnvironmentCallbacks(IResource resource)
  {
    DistributedApplicationExecutionContext execution = new(DistributedApplicationOperation.Run);
    Dictionary<string, object> variables = [];
    EnvironmentCallbackContext context = new(execution, resource, variables, CancellationToken.None)
    {
      Logger = NullLogger.Instance
    };

    foreach (EnvironmentCallbackAnnotation annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
    {
      await annotation.Callback(context);
    }

    return variables;
  }

  private static async Task<List<object>> ApplyArgumentCallbacks(IResource resource)
  {
    List<object> args = [];
    CommandLineArgsCallbackContext context = new(args, resource, CancellationToken.None)
    {
      Logger = NullLogger.Instance,
      ExecutionContext = new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run)
    };

    foreach (CommandLineArgsCallbackAnnotation annotation in resource.Annotations.OfType<CommandLineArgsCallbackAnnotation>())
    {
      await annotation.Callback(context);
    }

    return args;
  }
}
