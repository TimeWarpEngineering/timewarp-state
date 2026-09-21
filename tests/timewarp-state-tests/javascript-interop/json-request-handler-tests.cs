#region Purpose
// Proves JsonRequestHandler.InitAsync creates one DotNetObjectReference across N calls and disposes it.
#endregion

namespace JsonRequestHandlerTests;

public class Should_
{
  public async Task InitAsync_Called_Repeatedly_Creates_One_DotNetObjectReference()
  {
    RecordingJsRuntime recordingJsRuntime = new();
    JsonRequestHandler jsonRequestHandler = CreateHandler(recordingJsRuntime);

    for (int i = 0; i < 5; i++)
    {
      await jsonRequestHandler.InitAsync();
    }

    recordingJsRuntime.InvokeCount.ShouldBe(1);
    recordingJsRuntime.LastIdentifier.ShouldBe("InitializeJavaScriptInterop");
    recordingJsRuntime.LastArgs.ShouldNotBeNull();
    recordingJsRuntime.LastArgs.Length.ShouldBe(1);
    DotNetObjectReference<JsonRequestHandler> jsonRequestHandlerReference =
      recordingJsRuntime.LastArgs[0].ShouldBeOfType<DotNetObjectReference<JsonRequestHandler>>();
    jsonRequestHandlerReference.Value.ShouldBe(jsonRequestHandler);
  }

  public async Task Dispose_Releases_The_DotNetObjectReference()
  {
    RecordingJsRuntime recordingJsRuntime = new();
    JsonRequestHandler jsonRequestHandler = CreateHandler(recordingJsRuntime);
    await jsonRequestHandler.InitAsync();
    DotNetObjectReference<JsonRequestHandler> jsonRequestHandlerReference =
      recordingJsRuntime.LastArgs![0].ShouldBeOfType<DotNetObjectReference<JsonRequestHandler>>();

    jsonRequestHandler.Dispose();

    Should.Throw<ObjectDisposedException>(() => _ = jsonRequestHandlerReference.Value);
  }

  public async Task DisposeAsync_Releases_The_DotNetObjectReference()
  {
    RecordingJsRuntime recordingJsRuntime = new();
    JsonRequestHandler jsonRequestHandler = CreateHandler(recordingJsRuntime);
    await jsonRequestHandler.InitAsync();
    DotNetObjectReference<JsonRequestHandler> jsonRequestHandlerReference =
      recordingJsRuntime.LastArgs![0].ShouldBeOfType<DotNetObjectReference<JsonRequestHandler>>();

    await jsonRequestHandler.DisposeAsync();

    Should.Throw<ObjectDisposedException>(() => _ = jsonRequestHandlerReference.Value);
  }

  public static void Dispose_Without_Init_Does_Not_Throw()
  {
    JsonRequestHandler jsonRequestHandler = CreateHandler(new RecordingJsRuntime());
    Should.NotThrow(jsonRequestHandler.Dispose);
  }

  public async Task InitAsync_After_Dispose_Does_Not_Create_A_Reference()
  {
    RecordingJsRuntime recordingJsRuntime = new();
    JsonRequestHandler jsonRequestHandler = CreateHandler(recordingJsRuntime);
    jsonRequestHandler.Dispose();
    await jsonRequestHandler.InitAsync();
    recordingJsRuntime.InvokeCount.ShouldBe(0);
  }

  private static JsonRequestHandler CreateHandler(IJSRuntime jsRuntime)
  {
    return new
    (
      NullLogger<JsonRequestHandler>.Instance,
      null!,
      jsRuntime,
      new TimeWarpStateOptions(new ServiceCollection())
    );
  }

  private sealed class RecordingJsRuntime : IJSRuntime
  {
    public int InvokeCount { get; private set; }
    public string? LastIdentifier { get; private set; }
    public object?[]? LastArgs { get; private set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
      return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
      InvokeCount++;
      LastIdentifier = identifier;
      LastArgs = args;
      return default;
    }
  }
}
