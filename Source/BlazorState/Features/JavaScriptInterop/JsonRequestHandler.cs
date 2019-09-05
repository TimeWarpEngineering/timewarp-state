namespace BlazorState.Features.JavaScriptInterop
{
  using MediatR;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Text.Json;
  using System.Threading;
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
      Logger.LogDebug($"{GetType().Name}: constructor");
      Mediator = aMediator;
      JSRuntime = aJSRuntime;
      JsonSerializerOptions = aBlazorStateOptions.JsonSerializerOptions;
    }

    /// <summary>
    /// This will handle the Javascript interop
    /// </summary>
    /// <param name="aRequestAsJson"></param>
    [JSInvokable]
    public async void Handle(string aRequestTypeAssemblyQualifiedName, string aRequestAsJson = null)
    {
      if (string.IsNullOrWhiteSpace(aRequestTypeAssemblyQualifiedName))
        throw new ArgumentException("was Null or empty", nameof(aRequestTypeAssemblyQualifiedName));

      Logger.LogDebug($"{GetType().Name}: Handling request of type: {aRequestTypeAssemblyQualifiedName}: {aRequestAsJson}");

      var requestType = Type.GetType(aRequestTypeAssemblyQualifiedName);
      if (requestType == null)
        throw new ArgumentException($"Type not found with name {aRequestTypeAssemblyQualifiedName}", nameof(aRequestTypeAssemblyQualifiedName));
      else
        Logger.LogDebug($"{GetType().Name}: Type ({aRequestTypeAssemblyQualifiedName})  was found");

      object instance = JsonSerializer.Deserialize(aRequestAsJson, requestType, JsonSerializerOptions);

      _ = await SendToMediator(requestType, instance);
    }

    public async Task InitAsync()
    {
      Logger.LogDebug("Init JsonRequestHandler");
      const string InitializeJavaScriptInteropName = "InitializeJavaScriptInterop";
      Logger.LogDebug(InitializeJavaScriptInteropName);
      await JSRuntime.InvokeAsync<object>(InitializeJavaScriptInteropName, DotNetObjectReference.Create(this));
    }

    /// <summary>
    /// Equivelent to the following code just using generics everywhere and reflection.
    ///  return await Mediator.Send(aInstance)
    /// </summary>
    private async Task<object> SendToMediator(Type aRequestType, object aInstance)
    {
      string genericRequestInterfaceName = typeof(IRequest<int>).Name;

      MethodInfo sendMethodInfo = Mediator.GetType().GetMethod(nameof(Mediator.Send));
      Type responseType = aRequestType
        .GetInterfaces()
        .FirstOrDefault(aType => aType.Name == genericRequestInterfaceName)
        .GenericTypeArguments
        .First();

      Logger.LogDebug($"{GetType().Name}: The response type of this request should be {responseType.FullName}");

      MethodInfo sendGenericMethodInfo = sendMethodInfo.MakeGenericMethod(responseType);

      Logger.LogDebug($"{GetType().Name}: Invoking Mediator.Send");

      // https://stackoverflow.com/questions/39674988/how-to-call-a-generic-async-method-using-reflection
      var task = (Task)sendGenericMethodInfo.Invoke(Mediator, new object[] { aInstance, default(CancellationToken) });
      await task.ConfigureAwait(false);
      PropertyInfo resultProperty = task.GetType().GetProperty("Result");
      return resultProperty.GetValue(task);
    }
  }
}