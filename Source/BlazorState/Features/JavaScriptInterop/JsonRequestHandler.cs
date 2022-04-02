namespace BlazorState.Features.JavaScriptInterop;

using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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

    Task<object> result = SendToMediator(requestType, instance);
    Logger.LogDebug(EventIds.JsonRequestHandled, "Request Handled");
    return result;
  }

  public ValueTask<object> InitAsync()
  {
    Logger.LogDebug(EventIds.JsonRequestHandler_Initializing, "Initializing");
    const string InitializeJavaScriptInteropName = "InitializeJavaScriptInterop";
    return JSRuntime.InvokeAsync<object>(InitializeJavaScriptInteropName, DotNetObjectReference.Create(this));
  }

  /// <summary>
  /// Equivelent to the following code just using generics everywhere and reflection.
  ///  return await Mediator.Send(aInstance)
  /// </summary>
  private Task<object> SendToMediator(Type aRequestType, object aInstance)
  {
    string genericRequestInterfaceName = typeof(IRequest<int>).Name;

    MethodInfo sendMethodInfo = Mediator.GetType().GetMethods().First
    (
      aMethodInfo =>
        aMethodInfo.IsGenericMethodDefinition &&
        aMethodInfo.Name == nameof(Mediator.Send)
    );

    Type responseType = aRequestType
      .GetInterfaces()
      .FirstOrDefault(aType => aType.Name == genericRequestInterfaceName)
      .GenericTypeArguments
      .First();

    MethodInfo sendGenericMethodInfo = sendMethodInfo.MakeGenericMethod(responseType);

    return sendGenericMethodInfo.InvokeAsync(Mediator, new[] { aInstance, default(CancellationToken) });
  }
}
