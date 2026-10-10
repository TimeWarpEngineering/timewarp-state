#region Purpose
// JsonRequestHandler.InitAsync runs once, and Dispose does not throw when called twice or before init.
#endregion

#region Design
// A script runtime counts InitializeJavaScriptInterop calls. Task 063 fixed a leaked DotNetObjectReference
// without a regression test; the second InitAsync must not create another root.
#endregion

namespace JsonRequestHandlerLifecycle_;

public class Should_
{
  public async Task Init_Once_And_Dispose_Twice()
  {
    ScriptRuntime scriptRuntime = new();
    JsonRequestHandler handler = new
    (
      NullLogger<JsonRequestHandler>.Instance,
      new UnusedSender(),
      scriptRuntime,
      new TimeWarpStateOptions(new ServiceCollection()),
      new JavaScriptDispatchRegistry()
    );

    await handler.InitAsync();
    await handler.InitAsync();
    scriptRuntime.InitCalls.ShouldBe(1);

    handler.Dispose();
    handler.Dispose();

    await handler.InitAsync();
    scriptRuntime.InitCalls.ShouldBe(1);
  }

  private sealed class ScriptRuntime : IJSRuntime
  {
    public int InitCalls { get; private set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
      InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
      if (identifier != "InitializeJavaScriptInterop")
      {
        throw new InvalidOperationException(identifier);
      }

      InitCalls++;
      return new ValueTask<TValue>(default(TValue)!);
    }
  }

  private sealed class UnusedSender : ISender<ClientPipeline>
  {
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
      throw new NotSupportedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>
    (
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default
    ) =>
      throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }
}
