namespace BlazorState.Features.JavaScriptInterop
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Services;
  using MediatR;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  public class JsonRequestHandler
  {
    public JsonRequestHandler(
      ILogger<JsonRequestHandler> aLogger,
      IMediator aMediator,
      BlazorHostingLocation aBlazorHostingLocation)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name}: constructor");
      Mediator = aMediator;
      BlazorHostingLocation = aBlazorHostingLocation;
    }

    private ILogger Logger { get; }

    private IMediator Mediator { get; }
    private BlazorHostingLocation BlazorHostingLocation { get; }

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

      object instance = Deserialize(aRequestAsJson, requestType);

      object result = await SendToMediator(requestType, instance);

      // TODO: We should probably return the result. But I want logic in C# not in js so holding off.
    }

    private object Deserialize(string aRequestAsJson, Type aRequestType)
    {
      MethodInfo deserializeMethodInfo = typeof(Json).GetMethod(nameof(Json.Deserialize), BindingFlags.Public | BindingFlags.Static);
      MethodInfo deserializeGenericMethodInfo = deserializeMethodInfo.MakeGenericMethod(aRequestType);

      object instance = deserializeGenericMethodInfo.Invoke(null, new object[] { aRequestAsJson });
      if (instance == null)
        throw new Exception($"Could not De-serialize ({aRequestAsJson} into and instance of type {aRequestType.AssemblyQualifiedName})");

      Logger.LogDebug($"{GetType().Name}: request created of type {instance.GetType().FullName}");
      return instance;
    }

    /// <summary>
    /// Sends
    /// </summary>
    /// <remarks>Sends an instance of this item to JavaScript side
    /// </remarks>
    private async Task<object> SendToMediator(Type aRequestType, object aInstance)
    {
      // return Mediator.Send(aInstance) is what this does but uses generics everywhere.

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

    // TOOD 0.9.0 we will have to Inject IJSRuntime so this technique won't work for the test.
    public async Task InitAsync()
    {
      Console.WriteLine("Init JsonRequestHandler");
      if (BlazorHostingLocation.IsClientSide || // Only init if running in WASM 
          !Assembly.GetEntryAssembly().FullName.Contains("TestApp.Client.Integration.Tests")) // or for test case.
      {
        Console.WriteLine("InitializeJavaScriptInterop");
        const string InitializeJavaScriptInteropName = "InitializeJavaScriptInterop";
        await JSRuntime.Current.InvokeAsync<object>(InitializeJavaScriptInteropName, new DotNetObjectRef(this));
      }
    }
  }
}