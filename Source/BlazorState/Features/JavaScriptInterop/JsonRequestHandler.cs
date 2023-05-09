namespace BlazorState.Features.JavaScriptInterop;

public class JsonRequestHandler
{
  private readonly JsonSerializerOptions JsonSerializerOptions;
  private readonly IJSRuntime JSRuntime;
  private readonly ILogger Logger;
  private readonly IMediator Mediator;

  public JsonRequestHandler
  (
    ILogger<JsonRequestHandler> aLogger,
    IMediator aMediator,
    IJSRuntime aJSRuntime,
    BlazorStateOptions aBlazorStateOptions
  )
  {
    Logger = aLogger;
    Mediator = aMediator;
    JSRuntime = aJSRuntime;
    JsonSerializerOptions = aBlazorStateOptions.JsonSerializerOptions;
    if (Logger is null) throw new ArgumentNullException(nameof(Logger));
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
  /// <param name="aRequestTypeAssemblyQualifiedName"></param>
  /// <param name="aRequestAsJson"></param>
  [JSInvokable]
  public Task Handle(string aRequestTypeAssemblyQualifiedName, string aRequestAsJson = null)
  {
    if (string.IsNullOrWhiteSpace(aRequestTypeAssemblyQualifiedName))
      throw new ArgumentException("was Null or empty", nameof(aRequestTypeAssemblyQualifiedName));

    Logger.LogDebug
    (
      EventIds.JsonRequestReceived,
      "Handling request of type: {aRequestTypeAssemblyQualifiedName}: {aRequestAsJson}",
      aRequestTypeAssemblyQualifiedName,
      aRequestAsJson
    );

    var requestType = Type.GetType(aRequestTypeAssemblyQualifiedName);
    if (requestType is null)
    {
      Logger.LogError
      (
        EventIds.JsonRequestOfInvalidType,
        "Could not find type: {aRequestTypeAssemblyQualifiedName}",
        aRequestTypeAssemblyQualifiedName
      );
      throw new InvalidRequestTypeException("Could not find type", aRequestTypeAssemblyQualifiedName);
    }

    object instance = JsonSerializer.Deserialize(aRequestAsJson, requestType, JsonSerializerOptions);

    Task<object> result = Mediator.Send(instance);
    Logger.LogDebug(EventIds.JsonRequestHandled, "Request Handled");
    return result;
  }

  public ValueTask<object> InitAsync()
  {
    Logger.LogDebug(EventIds.JsonRequestHandler_Initializing, "Initializing");
    const string InitializeJavaScriptInteropName = "InitializeJavaScriptInterop";
    return JSRuntime.InvokeAsync<object>(InitializeJavaScriptInteropName, DotNetObjectReference.Create(this));
  }
}
