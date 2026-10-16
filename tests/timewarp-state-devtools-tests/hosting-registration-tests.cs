#region Purpose
// Proves AddTimeWarpStateDevTools starts the packaged inspector and labels the dashboard link.
#endregion

#region Design
// The builder is not started. The test assembly opts into the package targets, so the stamped
// path is the in-repo publish output under timewarp-state-devtools-app/tools. Nothing asserts a
// csproj path. URL and environment callbacks run against a seeded http endpoint so the dashboard
// text and TIMEWARP_STATE_DEVTOOLS_ENDPOINT are visible without the Aspire dashboard.
// AddTimeWarpStateDevToolsProject covers the explicit csproj switch.
// ResolveAppLaunchCommand is called with explicit inputs so the host order does not depend on
// this process. Publish mode is `--operation publish` on the builder.
// Run mode sets ASPNETCORE_ENVIRONMENT=Development. The ingest preflight is checked by launching
// the stamped tools dll with that value, the same shape AddExecutable uses. CORS origins from two
// inspected apps stay in one reference expression.
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
    devtools.Resource.Command.ShouldBe(TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand());
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

  public static async Task RunThePackagedInspectorAsDevelopment()
  {
    IDistributedApplicationBuilder builder = CreateBuilder();
    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    Dictionary<string, object> variables = await ApplyEnvironmentCallbacks(devtools.Resource);
    variables[TimeWarpStateDevToolsHosting.AspNetCoreEnvironmentVariable]
      .ShouldBe(TimeWarpStateDevToolsHosting.DevelopmentEnvironmentName);
  }

  public static async Task KeepCorsOriginsFromEveryInspectedApp()
  {
    string root = RepoPaths.Root();
    IDistributedApplicationBuilder builder = CreateBuilder();
    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    string serverA = Path.Combine(root, "samples/04-telemetry/server/sample-04-server/sample-04-server.csproj");
    string serverB = Path.Combine
    (
      root,
      "samples/00-state-action-handler/server/sample-00-server/sample-00-server.csproj"
    );
    builder.AddProject("frontend-a", serverA).WithTimeWarpStateDevTools(devtools);
    builder.AddProject("frontend-b", serverB).WithTimeWarpStateDevTools(devtools);

    Dictionary<string, object> variables = await ApplyEnvironmentCallbacks(devtools.Resource);
    ReferenceExpression expression = variables[TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable]
      .ShouldBeOfType<ReferenceExpression>();
    List<string> names = [];
    foreach (IValueProvider provider in expression.ValueProviders)
    {
      if (provider is EndpointReference endpoint)
      {
        names.Add(endpoint.Resource.Name);
      }
    }

    names.ShouldContain("frontend-a");
    names.ShouldContain("frontend-b");
  }

  public static async Task AnswerTheIngestPreflightWhenLaunchedLikeTheExecutable()
  {
    string appPath = StampedAppPath();
    File.Exists(appPath).ShouldBeTrue();
    IDistributedApplicationBuilder builder = CreateBuilder();
    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    Dictionary<string, object> variables = await ApplyEnvironmentCallbacks(devtools.Resource);
    string environmentName = variables[TimeWarpStateDevToolsHosting.AspNetCoreEnvironmentVariable]
      .ShouldBeOfType<string>();
    environmentName.ShouldBe(TimeWarpStateDevToolsHosting.DevelopmentEnvironmentName);

    const string origin = "http://localhost:5086";
    int port = FreePort();
    string? directory = Path.GetDirectoryName(appPath);
    directory.ShouldNotBeNullOrWhiteSpace();
    using CancellationTokenSource lifetime = new(TimeSpan.FromSeconds(45));
    Task<CommandOutput> running = Shell.Builder(devtools.Resource.Command)
      .WithArguments(appPath)
      .WithWorkingDirectory(directory)
      .WithEnvironmentVariable(TimeWarpStateDevToolsHosting.AspNetCoreEnvironmentVariable, environmentName)
      .WithEnvironmentVariable(TimeWarpStateDevToolsHosting.HttpPortsEnvironmentVariable, port.ToString())
      .WithEnvironmentVariable(DevToolsIngestCors.OriginsEnvironmentVariable, origin)
      .WithNoValidation()
      .CaptureAsync(lifetime.Token);
    try
    {
      using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(2) };
      HttpStatusCode? preflightStatus = null;
      bool allowed = false;
      bool pageAllows = true;
      DateTime deadline = DateTime.UtcNow.AddSeconds(40);
      while (DateTime.UtcNow < deadline)
      {
        if (running.IsCompleted)
        {
          CommandOutput exited = await running;
          throw new InvalidOperationException($"Inspector exited {exited.ExitCode}. {exited.Stderr}");
        }

        try
        {
          using HttpRequestMessage preflight = new(HttpMethod.Options, $"http://127.0.0.1:{port}/ingest/spans");
          preflight.Headers.TryAddWithoutValidation("Origin", origin);
          preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
          preflight.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "content-type");
          using HttpResponseMessage preflightResponse = await client.SendAsync(preflight);
          preflightStatus = preflightResponse.StatusCode;
          allowed = preflightResponse.Headers.TryGetValues("Access-Control-Allow-Origin", out IEnumerable<string>? values)
            && values.Contains(origin, StringComparer.Ordinal);
          if (preflightStatus != HttpStatusCode.NoContent || !allowed)
          {
            await Task.Delay(250);
            continue;
          }

          using HttpRequestMessage page = new(HttpMethod.Get, $"http://127.0.0.1:{port}/");
          page.Headers.TryAddWithoutValidation("Origin", origin);
          using HttpResponseMessage pageResponse = await client.SendAsync(page);
          pageAllows = pageResponse.Headers.Contains("Access-Control-Allow-Origin");
          break;
        }
        catch (HttpRequestException)
        {
          await Task.Delay(250);
        }
      }

      preflightStatus.ShouldBe(HttpStatusCode.NoContent);
      allowed.ShouldBeTrue();
      pageAllows.ShouldBeFalse();
    }
    finally
    {
      await lifetime.CancelAsync();
      try
      {
        await running;
      }
      catch (OperationCanceledException)
      {
      }
    }
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
      devtools.Resource.Command.ShouldBe(TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand());
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

    bool clientPublishesAnOrigin = server.Resource.Annotations
      .OfType<EndpointAnnotation>()
      .Any(endpoint =>
        string.Equals(endpoint.Name, "http", StringComparison.OrdinalIgnoreCase)
        || string.Equals(endpoint.Name, "https", StringComparison.OrdinalIgnoreCase));
    Dictionary<string, object> devtoolsVariables = await ApplyEnvironmentCallbacks(devtools.Resource);
    devtoolsVariables
      .ContainsKey(TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable)
      .ShouldBe(clientPublishesAnOrigin);
  }

  public static void ResolveTheDotNetHostInOrder()
  {
    const string hostPath = "/opt/dotnet/dotnet";
    const string muxer = "/usr/share/dotnet/dotnet";
    const string muxerExe = "/usr/share/dotnet/DOTNET.EXE";
    const string appHost = "/apps/TwDevToolsConsumer";
    const string root = "/preview/dotnet";
    string rootHost = Path.Combine(root, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");

    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand(hostPath, muxer, root).ShouldBe(hostPath);
    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand("  " + hostPath + "  ", appHost, null).ShouldBe(hostPath);
    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand(null, muxer, root).ShouldBe(muxer);
    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand("   ", muxerExe, root).ShouldBe(muxerExe);
    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand(null, appHost, root).ShouldBe(rootHost);
    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand(null, appHost, null)
      .ShouldBe(TimeWarpStateDevToolsHosting.AppLaunchCommand);
    TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand(null, null, "   ")
      .ShouldBe(TimeWarpStateDevToolsHosting.AppLaunchCommand);
  }

  public static async Task LeaveTheInspectedAppUnwiredWhenPublishing()
  {
    IDistributedApplicationBuilder builder = CreateBuilder(publish: true);
    builder.ExecutionContext.IsPublishMode.ShouldBeTrue();

    IResourceBuilder<ExecutableResource> devtools = builder.AddTimeWarpStateDevTools();
    string serverProject = Path.Combine
    (
      RepoPaths.Root(),
      "samples/04-telemetry/server/sample-04-server/sample-04-server.csproj"
    );
    IResourceBuilder<ProjectResource> server = builder
      .AddProject("sample-04-server", serverProject)
      .WithTimeWarpStateDevTools(devtools);

    server.Resource.Annotations.OfType<WaitAnnotation>()
      .ShouldNotContain(annotation => annotation.Resource == devtools.Resource);

    Dictionary<string, object> variables = await ApplyEnvironmentCallbacks(server.Resource);
    variables.ContainsKey(TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable).ShouldBeFalse();

    Dictionary<string, object> devtoolsVariables = await ApplyEnvironmentCallbacks(devtools.Resource);
    devtoolsVariables.ContainsKey(TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable).ShouldBeFalse();
    devtoolsVariables.ContainsKey(TimeWarpStateDevToolsHosting.AspNetCoreEnvironmentVariable).ShouldBeFalse();
  }

  public static void ShareTheEndpointVariableNameWithTheExporter()
  {
    TimeWarpStateDevToolsHosting.EndpointEnvironmentVariable
      .ShouldBe(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
  }

  public static void ShareTheCorsVariableNameWithTheInspector()
  {
    TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable
      .ShouldBe(DevToolsIngestCors.OriginsEnvironmentVariable);
  }

  private static int FreePort()
  {
    using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
    return ((IPEndPoint)socket.LocalEndPoint!).Port;
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

  private static IDistributedApplicationBuilder CreateBuilder(bool publish = false)
  {
    string[] args = publish ? ["--operation", "publish"] : [];
    return DistributedApplication.CreateBuilder
    (
      new DistributedApplicationOptions
      {
        Args = args,
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
