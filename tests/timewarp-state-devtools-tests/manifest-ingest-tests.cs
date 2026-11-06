#region Purpose
// Proves the manifest channel: the ingest route, the registry, the export URI and the no-op publisher.
#endregion

#region Design
// The endpoint is reachable by any local page, so the body is untrusted: JSON content type only, size capped,
// malformed bodies are 400 and invalid manifest entries are skipped. The publisher must never throw when DevTools
// is down, and the registration must do nothing when TIMEWARP_STATE_DEVTOOLS_ENDPOINT is unset.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class ManifestIngestEndpoint_Should
{
  public static async Task StoreAValidManifest()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    HttpResponseMessage response = await Post(client, Document(CounterManifest("App")));
    response.EnsureSuccessStatusCode();
    (await response.Content.ReadFromJsonAsync<int>()).ShouldBe(1);

    IReadOnlyList<StateManifest> stored = factory.Services.GetRequiredService<InspectorManifest>().Snapshot();
    stored.Count.ShouldBe(1);
    stored[0].AssemblyName.ShouldBe("App");
    stored[0].States[0].Name.ShouldBe("CounterState");
    stored[0].States[0].Actions[0].CatalogName.ShouldBe("Counter.Add");
  }

  public static async Task RejectANonJsonContentTypeWith415()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    using StringContent content = new("""{"Manifests":[]}""", Encoding.UTF8, "text/plain");
    HttpResponseMessage response = await client.PostAsync("/ingest/manifest", content);
    response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
  }

  public static async Task RejectAnOversizedBodyWith413()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    string padding = new('x', ManifestIngestEndpoint.MaxBodyBytes + 1024);
    using StringContent content = new($$"""{"Manifests":[],"Pad":"{{padding}}"}""", Encoding.UTF8, "application/json");
    HttpResponseMessage response = await client.PostAsync("/ingest/manifest", content);
    response.StatusCode.ShouldBe(HttpStatusCode.RequestEntityTooLarge);
  }

  public static async Task RejectMalformedJsonWith400()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    using StringContent content = new("{not json", Encoding.UTF8, "application/json");
    HttpResponseMessage response = await client.PostAsync("/ingest/manifest", content);
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  public static async Task RejectANullManifestsListWith400()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    using StringContent content = new("""{"Manifests":null}""", Encoding.UTF8, "application/json");
    HttpResponseMessage response = await client.PostAsync("/ingest/manifest", content);
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  public static async Task SkipInvalidManifestEntries()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    ManifestIngestDocument document = new();
    document.Manifests.Add("not json");
    document.Manifests.Add("""{"states":[]}""");
    document.Manifests.Add("[]");
    document.Manifests.Add(null!);
    document.Manifests.Add(new string('x', ManifestIngestEndpoint.MaxManifestLength + 1));
    document.Manifests.Add(CounterManifest("Good"));
    HttpResponseMessage response = await Post(client, document);
    response.EnsureSuccessStatusCode();
    (await response.Content.ReadFromJsonAsync<int>()).ShouldBe(1);
    factory.Services.GetRequiredService<InspectorManifest>().Snapshot().Select(manifest => manifest.AssemblyName).ShouldBe(["Good"]);
  }

  public static async Task ReadAtMostTheManifestsPerRequestLimit()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    ManifestIngestDocument document = new();
    for (int index = 0; index < ManifestIngestEndpoint.MaxManifestsPerRequest + 5; index++)
    {
      document.Manifests.Add(CounterManifest("App" + index));
    }

    HttpResponseMessage response = await Post(client, document);
    (await response.Content.ReadFromJsonAsync<int>()).ShouldBe(ManifestIngestEndpoint.MaxManifestsPerRequest);
  }

  internal static string CounterManifest(string assembly) =>
    $$"""{"version":1,"assembly":"{{assembly}}","states":[{"type":"App.CounterState","name":"CounterState","persistent":null,"clone":"generated","actions":[{"type":"App.CounterState+AddActionSet+Action","name":"AddActionSet.Action","method":"Add","parameters":[{"name":"amount","type":"int","default":"1","required":false}],"catalog":"Counter.Add","tracked":false}]}]}""";

  private static ManifestIngestDocument Document(params string[] manifests) =>
    new()
    {
      Manifests = [.. manifests]
    };

  private static Task<HttpResponseMessage> Post(HttpClient client, ManifestIngestDocument document) =>
    client.PostAsJsonAsync("/ingest/manifest", document, ManifestIngestJsonContext.Default.ManifestIngestDocument);
}

public class InspectorManifest_Should
{
  public static void ReplaceTheManifestOfTheSameAssemblyAndRaiseChanged()
  {
    InspectorManifest registry = new();
    int changes = 0;
    registry.Changed += () => changes++;

    registry.Set(StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("App"))).ShouldBeTrue();
    registry.Set(StateManifest.Parse("""{"version":1,"assembly":"App","states":[]}""")).ShouldBeTrue();

    changes.ShouldBe(2);
    IReadOnlyList<StateManifest> stored = registry.Snapshot();
    stored.Count.ShouldBe(1);
    stored[0].States.Count.ShouldBe(0);
  }

  public static void StoreABatchAndRaiseChangedOnce()
  {
    InspectorManifest registry = new();
    int changes = 0;
    registry.Changed += () => changes++;

    registry.SetAll
    (
      [
        StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("A")),
        StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("B")),
        StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("A"))
      ]
    ).ShouldBe(3);

    changes.ShouldBe(1);
    registry.Snapshot().Select(manifest => manifest.AssemblyName).ShouldBe(["A", "B"]);
    registry.SetAll([]).ShouldBe(0);
    changes.ShouldBe(1);
  }

  public static void RefuseANewAssemblyPastTheCap()
  {
    InspectorManifest registry = new();
    for (int index = 0; index < InspectorManifest.MaxAssemblies; index++)
    {
      registry.Set(StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("App" + index))).ShouldBeTrue();
    }

    registry.Set(StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("Extra"))).ShouldBeFalse();
    registry.Set(StateManifest.Parse(ManifestIngestEndpoint_Should.CounterManifest("App0"))).ShouldBeTrue();
    registry.Snapshot().Count.ShouldBe(InspectorManifest.MaxAssemblies);
  }
}

public class DevToolsManifestPublisher_Should
{
  public static void BuildTheManifestIngestUri()
  {
    TimeWarpStateDevToolsExport.ManifestIngestUri(new Uri("http://localhost:5288")).AbsoluteUri
      .ShouldBe("http://localhost:5288/ingest/manifest");
    TimeWarpStateDevToolsExport.ManifestIngestUri(new Uri("http://localhost:5288/")).AbsoluteUri
      .ShouldBe("http://localhost:5288/ingest/manifest");
  }

  public static void RegisterNothingWithoutTheEndpointVariable()
  {
    WithEndpoint(null, () =>
    {
      ServiceCollection services = new();
      services.AddTimeWarpStateDevToolsManifest(typeof(DevToolsManifestPublisher_Should).Assembly);
      services.Count.ShouldBe(0);
    });
  }

  public static void RegisterAHostedServiceWithTheEndpointVariable()
  {
    WithEndpoint("http://localhost:5288", () =>
    {
      ServiceCollection services = new();
      services.AddTimeWarpStateDevToolsManifest(typeof(DevToolsManifestPublisher_Should).Assembly);
      services.Count(descriptor => descriptor.ServiceType == typeof(IHostedService)).ShouldBe(1);
    });
  }

  public static void ContributeNothingFromAnAssemblyWithoutAManifest()
  {
    DevToolsManifestPublisher.CollectManifests([typeof(string).Assembly]).ShouldBeEmpty();
  }

  public static void CollectTheManifestOfAnAssemblyWithStates()
  {
    DevToolsManifestPublisher.CollectManifests([typeof(InspectorManifest).Assembly]).Count.ShouldBe(1);
  }

  public static async Task NotThrowWhenDevToolsIsDown()
  {
    // The DevTools assembly declares InspectorState, so it has a manifest and PublishAsync really sends a request.
    using DevToolsManifestPublisher publisher = new(new Uri("http://127.0.0.1:1/ingest/manifest"), [typeof(InspectorManifest).Assembly]);
    await publisher.StartAsync(CancellationToken.None);
    (await publisher.PublishAsync(CancellationToken.None)).ShouldBeFalse();
  }

  private static void WithEndpoint(string? value, Action action)
  {
    string? previous = Environment.GetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
    try
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable, value);
      action();
    }
    finally
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable, previous);
    }
  }
}
