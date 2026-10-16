#region Purpose
// Proves the browser exporter posts a store action to the inspector and stays quiet when opted out.
#endregion

#region Design
// The exporter listens on the calling thread and posts with SendAsync. The test passes the inspector
// TestServer handler so no socket is opened. A missing origin registers nothing. Outside Development,
// resolution does not call discovery. A down inspector is swallowed, and the flush task completes.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class DevToolsWasmExporter_Should
{
  public static async Task PostAWasmStoreActionIntoTheInspector()
  {
    await using WebApplicationFactory<Program> factory = new();
    using DevToolsWasmExporter exporter = new
    (
      new Uri(factory.Server.BaseAddress, "ingest/spans"),
      factory.Server.CreateHandler(),
      TimeSpan.FromMilliseconds(15)
    );

    using ActivitySource source = new(InspectorCapture.ActivitySourceName);
    using Activity activity = source.StartActivity("WasmCounterState.IncrementCount")!;
    activity.ShouldNotBeNull();
    activity.SetTag(InspectorCapture.StateTypeTagName, "WasmCounterState");
    activity.SetTag(InspectorCapture.ActionTagName, "IncrementCount");
    activity.Stop();

    InspectorLog log = factory.Services.GetRequiredService<InspectorLog>();
    ActionLogEntry? entry = null;
    for (int attempt = 0; attempt < 50 && entry is null; attempt++)
    {
      entry = log.Snapshot().FirstOrDefault(candidate => candidate.StateTypeName == "WasmCounterState");
      if (entry is null)
      {
        await Task.Delay(20);
      }
    }

    entry.ShouldNotBeNull();
    entry.ActionName.ShouldBe("IncrementCount");
    entry.Runtime.ShouldBe(InspectorCapture.WasmRuntime);
  }

  public static async Task SwallowADownInspector()
  {
    using ThrowingHandler handler = new();
    using DevToolsWasmExporter exporter = new
    (
      new Uri("http://127.0.0.1:9/ingest/spans"),
      handler,
      TimeSpan.FromMilliseconds(15)
    );
    using ActivitySource source = new(InspectorCapture.ActivitySourceName);
    using Activity activity = source.StartActivity("CounterState.Go")!;
    activity.ShouldNotBeNull();
    activity.SetTag(InspectorCapture.StateTypeTagName, "CounterState");
    activity.SetTag(InspectorCapture.ActionTagName, "Go");
    activity.Stop();

    for (int attempt = 0; attempt < 50 && handler.Calls == 0; attempt++)
    {
      await Task.Delay(20);
    }

    handler.Calls.ShouldBeGreaterThan(0);
    exporter.Dispose();
    await exporter.FlushTask.WaitAsync(TimeSpan.FromSeconds(2));
    exporter.FlushTask.IsCompletedSuccessfully.ShouldBeTrue();
  }

  public static void RegisterNothingWhenTheEndpointIsBlankOrNotAnOrigin()
  {
    ServiceCollection services = new();
    services.AddTimeWarpStateDevToolsWasmExporter(null);
    services.AddTimeWarpStateDevToolsWasmExporter("  ");
    services.AddTimeWarpStateDevToolsWasmExporter("not a uri");
    services.AddTimeWarpStateDevToolsWasmExporter("ftp://localhost");
    services.Any(descriptor => descriptor.ServiceType == typeof(DevToolsWasmExporter)).ShouldBeFalse();
  }

  public static void RegisterWhenTheEndpointIsAnOrigin()
  {
    ServiceCollection services = new();
    services.AddTimeWarpStateDevToolsWasmExporter("http://localhost:5288/ignored");
    ServiceDescriptor descriptor = services.ShouldHaveSingleItem();
    descriptor.ServiceType.ShouldBe(typeof(DevToolsWasmExporter));
    DevToolsWasmExporter exporter = descriptor.ImplementationInstance.ShouldBeOfType<DevToolsWasmExporter>();
    exporter.Dispose();
  }

  public static async Task SkipDiscoveryOutsideDevelopment()
  {
    RecordingHandler handler = new();
    string? endpoint = await TimeWarpStateDevToolsWasm.ResolveEndpointAsync
    (
      isDevelopment: false,
      configuredEndpoint: "http://localhost:5288",
      baseAddress: "http://localhost:5086",
      handler: handler
    );
    endpoint.ShouldBeNull();
    handler.Calls.ShouldBe(0);
  }

  public static async Task IgnoreABadConfiguredOriginWithoutDiscovery()
  {
    RecordingHandler handler = new();
    string? endpoint = await TimeWarpStateDevToolsWasm.ResolveEndpointAsync
    (
      isDevelopment: true,
      configuredEndpoint: "not a uri",
      baseAddress: "http://localhost:5086/",
      handler: handler
    );
    endpoint.ShouldBeNull();
    handler.Calls.ShouldBe(0);
  }

  public static async Task PreferConfigurationOverTheEnvironmentAndDiscovery()
  {
    RecordingHandler handler = new();
    await WithEndpoint("http://from-env.example", async () =>
    {
      string? endpoint = await TimeWarpStateDevToolsWasm.ResolveEndpointAsync
      (
        isDevelopment: true,
        configuredEndpoint: "https://configured.example/path",
        baseAddress: "http://localhost:5086/",
        handler: handler
      );
      endpoint.ShouldBe("https://configured.example");
      handler.Calls.ShouldBe(0);
    });
  }

  public static async Task DiscoverWhenConfigurationAndEnvironmentAreBlank()
  {
    RecordingHandler handler = new()
    {
      Body = """{"endpoint":"http://127.0.0.1:5288/"}"""
    };
    await WithEndpoint(null, async () =>
    {
      string? endpoint = await TimeWarpStateDevToolsWasm.ResolveEndpointAsync
      (
        isDevelopment: true,
        configuredEndpoint: "  ",
        baseAddress: "http://localhost:5086/",
        handler: handler
      );
      endpoint.ShouldBe("http://127.0.0.1:5288");
      handler.Calls.ShouldBe(1);
      handler.RequestUri.ShouldBe(new Uri("http://localhost:5086/_timewarp/state-devtools"));
    });
  }

  public static async Task DiscoverUnderANonRootBaseAddress()
  {
    RecordingHandler handler = new()
    {
      Body = """{"endpoint":"http://127.0.0.1:5288/"}"""
    };
    await WithEndpoint(null, async () =>
    {
      string? endpoint = await TimeWarpStateDevToolsWasm.ResolveEndpointAsync
      (
        isDevelopment: true,
        configuredEndpoint: "  ",
        baseAddress: "http://localhost:5086/app",
        handler: handler
      );
      endpoint.ShouldBe("http://127.0.0.1:5288");
      handler.Calls.ShouldBe(1);
      handler.RequestUri.ShouldBe(new Uri("http://localhost:5086/app/_timewarp/state-devtools"));
    });
  }

  public static void ShareTheIngestUriTheDiscoveryPathAndTheEndpointVariable()
  {
    Uri origin = new("http://localhost:5288");
    TimeWarpStateDevToolsWasm.IngestUri(origin).ShouldBe(TimeWarpStateDevToolsExport.IngestUri(origin));
    TimeWarpStateDevToolsWasm.DiscoveryPath.ShouldBe(TimeWarpStateDevToolsClientConfigEndpoint.Path);
    TimeWarpStateDevToolsWasm.EndpointEnvironmentVariable
      .ShouldBe(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
  }

  private static async Task WithEndpoint(string? value, Func<Task> action)
  {
    string? previous = Environment.GetEnvironmentVariable(TimeWarpStateDevToolsWasm.EndpointEnvironmentVariable);
    try
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsWasm.EndpointEnvironmentVariable, value);
      await action();
    }
    finally
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsWasm.EndpointEnvironmentVariable, previous);
    }
  }

  private sealed class RecordingHandler : HttpMessageHandler
  {
    public int Calls { get; private set; }

    public Uri? RequestUri { get; private set; }

    public string Body { get; set; } = """{"endpoint":"http://localhost:5288"}""";

    protected override Task<HttpResponseMessage> SendAsync
    (
      HttpRequestMessage request,
      CancellationToken cancellationToken
    )
    {
      Calls++;
      RequestUri = request.RequestUri;
      HttpResponseMessage response = new(HttpStatusCode.OK)
      {
        Content = new StringContent(Body, Encoding.UTF8, "application/json")
      };
      return Task.FromResult(response);
    }
  }

  private sealed class ThrowingHandler : HttpMessageHandler
  {
    private int calls;

    public int Calls => Volatile.Read(ref calls);

    protected override Task<HttpResponseMessage> SendAsync
    (
      HttpRequestMessage request,
      CancellationToken cancellationToken
    )
    {
      Interlocked.Increment(ref calls);
      throw new HttpRequestException("inspector down");
    }
  }
}
