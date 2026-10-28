#region Purpose
// Posts completed TimeWarp.State activities from the browser to the DevTools ingest endpoint.
#endregion

#region Design
// The server exporter's BatchActivityExportProcessor runs on a timer thread and calls HttpClient.Send.
// Browser WebAssembly has one thread and rejects that synchronous send. This type listens with
// ActivityListener, copies the span on ActivityStopped, and posts with HttpClient.SendAsync from an
// async loop started on the caller. The loop coalesces for a short delay, matching the server batch
// window. A full queue drops the oldest span. Every network and HTTP failure is swallowed, including
// cancellation, so a stopped inspector cannot throw into the app. The last queued batch can be lost
// when the page goes away: there is no background thread to finish it, and sendBeacon is not used.
// The listener accepts only TimeWarp.State. Framework component spans on
// Microsoft.AspNetCore.Components stay in the browser activity tree and are not posted.
// Each posted item is stamped timewarp.state.runtime=wasm. FlushTask is public so tests can
// assert the loop completed and did not fault. InternalsVisibleTo would also expose the internal
// copies of the ingest document this package compiles, which collide with the public exporter
// types. Dispose does not wait for FlushTask: on WebAssembly that wait deadlocks the single thread.
#endregion

namespace TimeWarp.State.DevTools.Wasm;

/// <summary>
/// Exports <see cref="Activity"/> spans from the browser to <c>POST /ingest/spans</c>.
/// </summary>
public sealed class DevToolsWasmExporter : IDisposable
{
  private readonly HttpClient HttpClient;
  private readonly bool DisposeHandler;
  private readonly Uri IngestUri;
  private readonly TimeSpan CoalesceDelay;
  private readonly Channel<SpanIngestItem> Queue;
  private readonly ActivityListener Listener;
  private readonly CancellationTokenSource Stop = new();
  /// <summary>
  /// Completes when the flush loop stops. A failed post does not fault it.
  /// Waiting on it from the browser deadlocks the single thread. <see cref="Dispose"/> does not wait.
  /// </summary>
  public Task FlushTask { get; }
  private int Disposed;

  /// <summary>
  /// Starts listening and posting to <paramref name="ingestUri"/>.
  /// </summary>
  /// <param name="ingestUri">Absolute <c>/ingest/spans</c> URI.</param>
  /// <param name="handler">
  /// HTTP handler. Null uses the default handler, which is the browser fetch handler in WebAssembly.
  /// The exporter does not dispose a handler that the caller passed in.
  /// </param>
  /// <param name="coalesceDelay">How long to wait after the first queued span before posting. Default is 200 ms.</param>
  /// <exception cref="ArgumentNullException"><paramref name="ingestUri"/> is null.</exception>
  /// <exception cref="ArgumentException"><paramref name="ingestUri"/> is not absolute.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="coalesceDelay"/> is negative.</exception>
  public DevToolsWasmExporter(Uri ingestUri, HttpMessageHandler? handler = null, TimeSpan? coalesceDelay = null)
  {
    ArgumentNullException.ThrowIfNull(ingestUri);
    if (!ingestUri.IsAbsoluteUri)
    {
      throw new ArgumentException("Ingest URI must be absolute.", nameof(ingestUri));
    }

    TimeSpan delay = coalesceDelay ?? TimeSpan.FromMilliseconds(200);
    if (delay < TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(coalesceDelay), "Coalesce delay cannot be negative.");
    }

    IngestUri = ingestUri;
    CoalesceDelay = delay;
    DisposeHandler = handler is null;
    HttpClient = handler is null
      ? new HttpClient { Timeout = TimeSpan.FromSeconds(2) }
      : new HttpClient(handler, disposeHandler: false)
      {
        Timeout = TimeSpan.FromSeconds(2)
      };

    Queue = Channel.CreateBounded<SpanIngestItem>
    (
      new BoundedChannelOptions(512)
      {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
        SingleWriter = false
      }
    );

    Listener = new ActivityListener
    {
      ShouldListenTo = static source => string.Equals
      (
        source.Name,
        InspectorCapture.ActivitySourceName,
        StringComparison.Ordinal
      ),
      Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
      ActivityStopped = OnStopped
    };
    ActivitySource.AddActivityListener(Listener);
    FlushTask = FlushLoopAsync();
  }

  /// <inheritdoc />
  public void Dispose()
  {
    if (Interlocked.Exchange(ref Disposed, 1) != 0)
    {
      return;
    }

    Listener.Dispose();
    Stop.Cancel();
    Queue.Writer.TryComplete();
    if (DisposeHandler)
    {
      HttpClient.Dispose();
    }

    // Do not wait for FlushTask. On WebAssembly that wait deadlocks the single thread.
    GC.SuppressFinalize(this);
  }

  private void OnStopped(Activity activity)
  {
    if (Volatile.Read(ref Disposed) != 0)
    {
      return;
    }

    try
    {
      SpanIngestItem? item = SpanIngestMapping.FromActivity(activity);
      if (item is null)
      {
        return;
      }

      item.Attributes[InspectorCapture.RuntimeTagName] = InspectorCapture.WasmRuntime;
      Queue.Writer.TryWrite(item);
    }
    catch (Exception exception) when (exception is not OutOfMemoryException)
    {
    }
  }

  private async Task FlushLoopAsync()
  {
    try
    {
      while (await Queue.Reader.WaitToReadAsync(Stop.Token).ConfigureAwait(false))
      {
        if (CoalesceDelay > TimeSpan.Zero)
        {
          await Task.Delay(CoalesceDelay, Stop.Token).ConfigureAwait(false);
        }

        List<SpanIngestItem> batch = [];
        while (batch.Count < 64 && Queue.Reader.TryRead(out SpanIngestItem? item))
        {
          batch.Add(item);
        }

        if (batch.Count > 0)
        {
          await PostAsync(batch).ConfigureAwait(false);
        }
      }
    }
    catch (Exception exception) when (exception is not OutOfMemoryException)
    {
    }
  }

  private async Task PostAsync(List<SpanIngestItem> spans)
  {
    try
    {
      SpanIngestDocument document = new()
      {
        Spans = spans
      };
      string json = JsonSerializer.Serialize(document, SpanIngestJsonContext.Default.SpanIngestDocument);
      using HttpRequestMessage request = new(HttpMethod.Post, IngestUri)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };
      HttpResponseMessage response = await HttpClient.SendAsync(request, Stop.Token).ConfigureAwait(false);
      response.Dispose();
    }
    catch (Exception exception) when (exception is not OutOfMemoryException)
    {
    }
  }
}
