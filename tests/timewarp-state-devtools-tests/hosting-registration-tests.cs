#region Purpose
// Proves AddTimeWarpStateDevTools adds the project and labels the dashboard link.
#endregion

#region Design
// The builder is not started. One test points at a csproj with no Properties/launchSettings.json and
// asserts the http EndpointAnnotation comes from AddTimeWarpStateDevTools, with nothing seeded.
// The in-repo project keeps a single http endpoint and the launch-profile port when the profile is
// also applied. URL and environment callbacks are invoked with a seeded http endpoint so the
// dashboard text and TIMEWARP_STATE_DEVTOOLS_ENDPOINT are visible without the Aspire dashboard.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class AddTimeWarpStateDevTools_Should
{
  public static void FindTheInRepoProjectFromTheSampleAppHost()
  {
    string root = RepoPaths.Root();
    string resolved = DevToolsProjectLocator.Resolve(Path.Combine(root, "samples/04-telemetry/apphost"));
    resolved.ShouldBe(Path.Combine(root, DevToolsProjectLocator.RelativeProjectPath));
  }

  public static void HonorTheProjectPathOverride()
  {
    string project = Path.Combine(RepoPaths.Root(), DevToolsProjectLocator.RelativeProjectPath);
    string? previous = Environment.GetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable);
    try
    {
      Environment.SetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable, project);
      DevToolsProjectLocator.Resolve("/tmp").ShouldBe(Path.GetFullPath(project));
    }
    finally
    {
      Environment.SetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable, previous);
    }
  }

  public static void DeclareTheHttpEndpointWithoutALaunchProfile()
  {
    string directory = Directory.CreateTempSubdirectory("tw-devtools-host-").FullName;
    string project = Path.Combine(directory, DevToolsProjectLocator.ProjectFileName);
    File.WriteAllText
    (
      project,
      """
      <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
          <TargetFramework>net11.0</TargetFramework>
        </PropertyGroup>
      </Project>
      """
    );

    string? previous = Environment.GetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable);
    Environment.SetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable, project);
    try
    {
      IDistributedApplicationBuilder builder = CreateBuilder(RepoPaths.Root());
      IResourceBuilder<ProjectResource> devtools = builder.AddTimeWarpStateDevTools();
      EndpointAnnotation httpEndpoint = HttpEndpoint(devtools.Resource);
      httpEndpoint.UriScheme.ShouldBe("http");
      httpEndpoint.Port.ShouldBeNull();
    }
    finally
    {
      Environment.SetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable, previous);
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task LabelTheDashboardLinkAndPublishTheEndpoint()
  {
    string root = RepoPaths.Root();
    string? previous = Environment.GetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable);
    Environment.SetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable, null);
    try
    {
      IDistributedApplicationBuilder builder = CreateBuilder(root);

      IResourceBuilder<ProjectResource> devtools = builder.AddTimeWarpStateDevTools();
      EndpointAnnotation httpEndpoint = HttpEndpoint(devtools.Resource);
      httpEndpoint.UriScheme.ShouldBe("http");
      httpEndpoint.Port.ShouldBe(5288);
      devtools.Resource.Name.ShouldBe(TimeWarpStateDevToolsHosting.DefaultResourceName);
      devtools.Resource.Annotations.OfType<IProjectMetadata>()
        .Single()
        .ProjectPath
        .ShouldBe(Path.Combine(root, DevToolsProjectLocator.RelativeProjectPath));

      string serverProject = Path.Combine(root, "samples/04-telemetry/server/sample-04-server/sample-04-server.csproj");
      IResourceBuilder<ProjectResource> server = builder
        .AddProject("sample-04-server", serverProject)
        .WithTimeWarpStateDevTools(devtools);

      server.Resource.Annotations.OfType<WaitAnnotation>()
        .ShouldContain(annotation => annotation.Resource == devtools.Resource);

      // WithUrlForEndpoint runs when Aspire publishes endpoint URLs. Each callback sees a seeded
      // http endpoint so the dashboard text is visible without starting the resource.
      (await DashboardLabel(devtools.Resource)).ShouldBe(TimeWarpStateDevToolsHosting.DashboardLinkText);

      Dictionary<string, object> variables = await ApplyEnvironmentCallbacks(server.Resource);
      variables.ContainsKey(TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable).ShouldBeTrue();
    }
    finally
    {
      Environment.SetEnvironmentVariable(DevToolsProjectLocator.ProjectPathEnvironmentVariable, previous);
    }
  }

  private static IDistributedApplicationBuilder CreateBuilder(string root)
  {
    return DistributedApplication.CreateBuilder
    (
      new DistributedApplicationOptions
      {
        Args = [],
        DisableDashboard = true,
        AllowUnsecuredTransport = true,
        AssemblyName = "TimeWarp.State.DevTools.Tests",
        ProjectDirectory = Path.Combine(root, "samples/04-telemetry/apphost")
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

  public static void ShareTheEndpointVariableNameWithTheExporter()
  {
    TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable
      .ShouldBe(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
  }
}
