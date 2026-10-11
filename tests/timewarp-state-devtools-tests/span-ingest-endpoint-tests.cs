#region Purpose
// Proves POST /ingest/spans rejects hostile input and survives null fields.
#endregion

#region Design
// The endpoint is reachable by any local page, so the body is untrusted: JSON content type only,
// size capped, and null members never reach the inspector page.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class SpanIngestEndpoint_Should
{
  public static async Task RejectANonJsonContentTypeWith415()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    using StringContent content = new("""{"Spans":[]}""", System.Text.Encoding.UTF8, "text/plain");
    HttpResponseMessage response = await client.PostAsync("/ingest/spans", content);
    response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
  }

  public static async Task RejectAnOversizedBodyWith413()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    string padding = new('x', SpanIngestEndpoint.MaxBodyBytes + 1024);
    using StringContent content = new($$"""{"Spans":[],"Pad":"{{padding}}"}""", System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage response = await client.PostAsync("/ingest/spans", content);
    response.StatusCode.ShouldBe(HttpStatusCode.RequestEntityTooLarge);
  }

  public static async Task RejectAChunkedOversizedBodyWith413()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    string padding = new('x', SpanIngestEndpoint.MaxBodyBytes + 1024);
    byte[] bytes = System.Text.Encoding.UTF8.GetBytes($$"""{"Spans":[],"Pad":"{{padding}}"}""");
    using HttpRequestMessage request = new(HttpMethod.Post, "/ingest/spans")
    {
      Content = new ChunkedContent(bytes)
    };
    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
    HttpResponseMessage response = await client.SendAsync(request);
    response.StatusCode.ShouldBe(HttpStatusCode.RequestEntityTooLarge);
  }

  public static async Task CapSpansPerRequest()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    SpanIngestDocument document = new();
    for (int index = 0; index < SpanIngestEndpoint.MaxSpansPerRequest + 20; index++)
    {
      document.Spans.Add(AppSpan("Action" + index));
    }

    HttpResponseMessage response = await client.PostAsJsonAsync
    (
      "/ingest/spans",
      document,
      SpanIngestJsonContext.Default.SpanIngestDocument
    );
    response.EnsureSuccessStatusCode();
    (await response.Content.ReadFromJsonAsync<int>()).ShouldBe(SpanIngestEndpoint.MaxSpansPerRequest);
  }

  public static async Task RejectANullSpansListWith400()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    using StringContent content = new("""{"Spans":null}""", System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage response = await client.PostAsync("/ingest/spans", content);
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  public static async Task NormalizeNullFieldsAndSkipNullItems()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    const string body = """
      {"Spans":[
        null,
        {"Name":null,"Status":null,"SpanId":null,"Attributes":null,"Events":null},
        {"Name":"CounterState.Go","Status":null,"Attributes":{"timewarp.state.state_type":"CounterState","timewarp.state.action":"Go"},"Events":[null,{"Name":"state.snapshot","Attributes":null},{"Name":"state.snapshot","Attributes":{"snapshot.json":null}}]}
      ]}
      """;
    using StringContent content = new(body, System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage response = await client.PostAsync("/ingest/spans", content);
    response.EnsureSuccessStatusCode();

    InspectorLog log = factory.Services.GetRequiredService<InspectorLog>();
    log.Snapshot().Count.ShouldBe(2);
    foreach (ActionLogEntry entry in log.Snapshot())
    {
      entry.Name.ShouldNotBeNull();
      entry.Status.ShouldNotBeNull();
      entry.ActionName.ShouldNotBeNull();
      entry.StateTypeName.ShouldNotBeNull();
    }
  }

  public static async Task CapAndMarkAnOversizedSnapshot()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    SpanIngestItem item = AppSpan("Big");
    item.Events[0].Attributes[InspectorCapture.SnapshotJsonTagName] = new string('y', SpanIngestEndpoint.MaxSnapshotLength + 500);
    SpanIngestDocument document = new() { Spans = [item] };

    HttpResponseMessage response = await client.PostAsJsonAsync
    (
      "/ingest/spans",
      document,
      SpanIngestJsonContext.Default.SpanIngestDocument
    );
    response.EnsureSuccessStatusCode();

    ActionLogEntry entry = factory.Services.GetRequiredService<InspectorLog>().Snapshot().Single();
    entry.SnapshotJson!.Length.ShouldBe(SpanIngestEndpoint.MaxSnapshotLength);
    entry.SnapshotTruncated.ShouldBeTrue();
  }

  private static SpanIngestItem AppSpan(string actionName) =>
    new()
    {
      Name = "CounterState." + actionName,
      Status = "Ok",
      Attributes = new Dictionary<string, string>
      {
        [InspectorCapture.ActionTagName] = actionName,
        [InspectorCapture.StateTypeTagName] = "CounterState"
      },
      Events =
      [
        new SpanIngestEvent
        {
          Name = InspectorCapture.SnapshotEventName,
          Attributes = new Dictionary<string, string>
          {
            [InspectorCapture.SnapshotJsonTagName] = "{}"
          }
        }
      ]
    };

  /// <summary>Content with no known length, so HttpClient sends it chunked.</summary>
  private sealed class ChunkedContent(byte[] bytes) : HttpContent
  {
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
      stream.WriteAsync(bytes, 0, bytes.Length);

    protected override bool TryComputeLength(out long length)
    {
      length = 0;
      return false;
    }
  }
}
