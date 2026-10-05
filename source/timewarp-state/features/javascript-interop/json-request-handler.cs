#region Purpose
// Dispatches JSON requests from JavaScript into the client mediator pipeline.
#endregion

#region Design
// Handle resolves names only against JavaScriptDispatchRegistry (never Type.GetType) and fails
// closed: unknown names, non-action types and unconstructible payloads log a warning and throw
// InvalidRequestTypeException. See javascript-dispatch-registry.cs for the opt-in rationale.
// InitAsync is idempotent so extra renders cannot leak JS interop roots.
// One DotNetObjectReference is stored for the handler lifetime and disposed
// with the scoped service. JSDisconnectedException is swallowed because
// circuit teardown can dispose after the JS runtime is already gone.
// TimeWarpJavaScriptInterop also gates on firstRender; both guards stay.
#endregion

namespace TimeWarp.Features.JavaScriptInterop;

public class JsonRequestHandler : IAsyncDisposable, IDisposable
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly IJSRuntime JsRuntime;
  private readonly ILogger Logger;
  private readonly ISender<ClientPipeline> Sender;
  private readonly JavaScriptDispatchRegistry JavaScriptDispatchRegistry;
  private bool IsDisposed;
  private bool IsInitialized;
  private DotNetObjectReference<JsonRequestHandler>? JsonRequestHandlerReference;

  public JsonRequestHandler
  (
    ILogger<JsonRequestHandler> logger,
    ISender<ClientPipeline> sender,
    IJSRuntime jsRuntime,
    TimeWarpStateOptions timeWarpStateOptions,
    JavaScriptDispatchRegistry javaScriptDispatchRegistry
  )
  {
    ArgumentNullException.ThrowIfNull(logger);
    ArgumentNullException.ThrowIfNull(javaScriptDispatchRegistry);
    Logger = logger;
    JavaScriptDispatchRegistry = javaScriptDispatchRegistry;
    Sender = sender;
    JsRuntime = jsRuntime;
    JsonSerializerOptions = timeWarpStateOptions.JsonSerializerOptions;
    Logger.LogDebug
    (
      EventIds.JsonRequestHandler_Initializing,
      "constructed with {JsonSerializerOptions}",
      JsonSerializer.Serialize(JsonSerializerOptions)
    );
  }

  /// <summary>
  /// Dispatches a request from JavaScript. Only types allow-listed with
  /// <see cref="ServiceCollectionExtensions.AddJavaScriptDispatch"/> (and Redux DevTools requests when
  /// DevTools is enabled) are dispatched; everything else is rejected.
  /// </summary>
  /// <param name="requestTypeAssemblyQualifiedName">
  /// The allowed type's full name, "FullName, AssemblyName", assembly-qualified name, or alias.
  /// </param>
  /// <param name="requestAsJson">The request as JSON. Empty creates the request with its parameterless constructor.</param>
  /// <exception cref="InvalidRequestTypeException">The name is not allowed, the type is not dispatchable, or the payload is invalid.</exception>
  [JSInvokable]
  public Task Handle(string requestTypeAssemblyQualifiedName, string? requestAsJson = null)
  {
    if (string.IsNullOrWhiteSpace(requestTypeAssemblyQualifiedName))
      throw new ArgumentException("was Null or empty", nameof(requestTypeAssemblyQualifiedName));

    Logger.LogDebug
    (
      EventIds.JsonRequestReceived,
      "Handling request of type: {requestTypeAssemblyQualifiedName}: {requestAsJson}",
      requestTypeAssemblyQualifiedName,
      requestAsJson
    );

    if (!JavaScriptDispatchRegistry.TryResolve(requestTypeAssemblyQualifiedName, out Type? requestType))
    {
      Logger.LogWarning
      (
        EventIds.JsonRequestOfInvalidType,
        "Rejected JavaScript dispatch of {requestTypeAssemblyQualifiedName}: not allowed. Allow it with AddJavaScriptDispatch",
        requestTypeAssemblyQualifiedName
      );
      throw new InvalidRequestTypeException
      (
        "Request type is not allowed for JavaScript dispatch. Allow it with services.AddJavaScriptDispatch(b => b.Allow<TAction>()).",
        requestTypeAssemblyQualifiedName
      );
    }

    if (!typeof(IAction).IsAssignableFrom(requestType) && !typeof(IReduxRequest).IsAssignableFrom(requestType))
    {
      Logger.LogWarning
      (
        EventIds.JsonRequestOfInvalidType,
        "Rejected JavaScript dispatch of {requestTypeAssemblyQualifiedName}: {requestType} is not an action",
        requestTypeAssemblyQualifiedName,
        requestType.FullName
      );
      throw new InvalidRequestTypeException("Request type is not an action.", requestTypeAssemblyQualifiedName);
    }

    object instance = CreateRequest(requestTypeAssemblyQualifiedName, requestType, requestAsJson);

    Task<object?> result = Sender.Send(instance);
    Logger.LogDebug(EventIds.JsonRequestHandled, "Request Handled");
    return result;
  }

  private object CreateRequest(string requestTypeName, Type requestType, string? requestAsJson)
  {
    Exception? innerException = null;
    try
    {
      object? instance = string.IsNullOrWhiteSpace(requestAsJson)
        ? requestType.GetConstructor(Type.EmptyTypes) is null ? null : Activator.CreateInstance(requestType)
        : JsonSerializer.Deserialize(requestAsJson, requestType, JsonSerializerOptions);

      if (instance is not null) return instance;
    }
    catch (Exception exception) when (exception is JsonException or NotSupportedException or TargetInvocationException or MissingMethodException)
    {
      innerException = exception;
    }

    Logger.LogWarning
    (
      EventIds.JsonRequestInvalidPayload,
      innerException,
      "Rejected JavaScript dispatch of {requestTypeAssemblyQualifiedName}: could not create the request from {requestAsJson}",
      requestTypeName,
      requestAsJson
    );
    throw new InvalidRequestTypeException
    (
      string.IsNullOrWhiteSpace(requestAsJson)
        ? "Request has no JSON payload and no public parameterless constructor."
        : "Request JSON could not be deserialized.",
      requestTypeName,
      innerException
    );
  }

  public ValueTask<object> InitAsync()
  {
    if (IsInitialized || IsDisposed)
    {
      return default;
    }

    Logger.LogDebug(EventIds.JsonRequestHandler_Initializing, "Initializing");
    JsonRequestHandlerReference = DotNetObjectReference.Create(this);
    IsInitialized = true;
    const string initializeJavaScriptInteropName = "InitializeJavaScriptInterop";
    return JsRuntime.InvokeAsync<object>(initializeJavaScriptInteropName, JsonRequestHandlerReference);
  }

  public void Dispose()
  {
    DisposeCore();
    GC.SuppressFinalize(this);
  }

  public ValueTask DisposeAsync()
  {
    Dispose();
    return ValueTask.CompletedTask;
  }

  private void DisposeCore()
  {
    if (IsDisposed)
    {
      return;
    }

    IsDisposed = true;
    try
    {
      JsonRequestHandlerReference?.Dispose();
    }
    catch (JSDisconnectedException)
    {
    }

    JsonRequestHandlerReference = null;
  }
}
