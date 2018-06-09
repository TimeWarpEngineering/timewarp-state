namespace BlazorState.Features.JavaScriptInterop
{
  using System;
  using MediatR;
  using Microsoft.AspNetCore.Blazor;
  using Microsoft.Extensions.Logging;

  public class JsonRequestHandler
  {
    public JsonRequestHandler(
      ILogger<JsonRequestHandler> aLogger,
      IMediator aMediator)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name}: constructor");
      Mediator = aMediator;
    }

    private ILogger Logger { get; }
    private IMediator Mediator { get; }

    public async void Handle(string aRequestAsJson)
    {
      Logger.LogDebug($"{GetType().Name}: Handling: {aRequestAsJson}");
      BaseJsonRequest baseRequest = JsonUtil.Deserialize<BaseJsonRequest>(aRequestAsJson);
      Logger.LogDebug($"{GetType().Name}: RequestType: {baseRequest.RequestType}");
      var requestType = Type.GetType(baseRequest.RequestType);
      if (requestType == null)
      {
        Logger.LogDebug($"{GetType().Name}: Type not found with name {baseRequest.RequestType}");
        return;
      }

      var request = (IRequest)Activator.CreateInstance(requestType, aRequestAsJson);
      if (request != null)
      {
        Logger.LogDebug($"{GetType().Name}: request created of type {request.GetType().FullName}");
        await Mediator.Send(request);
      }
    }
  }
}