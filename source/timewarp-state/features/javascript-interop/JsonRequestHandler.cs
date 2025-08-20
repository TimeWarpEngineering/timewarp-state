namespace TimeWarp.Features.JavaScriptInterop;

public class JsonRequestHandler
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly IJSRuntime JsRuntime;
  private readonly ILogger Logger;
  private readonly ISender Sender;

  public JsonRequestHandler
  (
    ILogger<JsonRequestHandler> logger,
    ISender sender,
    IJSRuntime jsRuntime,
    TimeWarpStateOptions timeWarpStateOptions
  )
  {
    ArgumentNullException.ThrowIfNull(logger);
    Logger = logger;
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
  /// This will handle the Javascript interop
  /// </summary>
  /// <param name="requestTypeAssemblyQualifiedName"></param>
  /// <param name="requestAsJson"></param>
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

    var requestType = Type.GetType(requestTypeAssemblyQualifiedName);
    if (requestType is null)
    {
      Logger.LogError
      (
        EventIds.JsonRequestOfInvalidType,
        "Could not find type: {aRequestTypeAssemblyQualifiedName}",
        requestTypeAssemblyQualifiedName
      );
      throw new InvalidRequestTypeException("Could not find type", requestTypeAssemblyQualifiedName);
    }

    object instance;
    if (string.IsNullOrWhiteSpace(requestAsJson))
    {
      instance = Activator.CreateInstance(requestType) ?? 
        throw new InvalidOperationException($"Cannot create an instance of {requestTypeAssemblyQualifiedName}. Ensure it has a parameterless constructor.");
    }
    else
    {
      instance = JsonSerializer.Deserialize(requestAsJson, requestType, JsonSerializerOptions) ?? throw new InvalidOperationException("Deserialization resulted in a null object.");
    }
    
    Task<object?> result = Sender.Send(instance);
    Logger.LogDebug(EventIds.JsonRequestHandled, "Request Handled");
    return result;
  }

  public ValueTask<object> InitAsync()
  {
    Logger.LogDebug(EventIds.JsonRequestHandler_Initializing, "Initializing");
    const string initializeJavaScriptInteropName = "InitializeJavaScriptInterop";
    return JsRuntime.InvokeAsync<object>(initializeJavaScriptInteropName, DotNetObjectReference.Create(this));
  }
}
