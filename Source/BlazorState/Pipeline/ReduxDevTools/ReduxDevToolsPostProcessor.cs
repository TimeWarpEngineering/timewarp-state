namespace BlazorState.Pipeline.ReduxDevTools;

using BlazorState;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ReduxDevToolsPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger Logger;
  private readonly ReduxDevToolsInterop ReduxDevToolsInterop;
  private readonly IReduxDevToolsStore Store;

  public ReduxDevToolsPostProcessor
  (
    ILogger<ReduxDevToolsPostProcessor<TRequest, TResponse>> aLogger,
    ReduxDevToolsInterop aReduxDevToolsInterop,
    IReduxDevToolsStore aStore
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.ReduxDevToolsPostProcessor_Constructing, "constructing");
    Store = aStore;
    ReduxDevToolsInterop = aReduxDevToolsInterop;
  }

  public async Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
  {
    try
    {
      Logger.LogDebug(EventIds.ReduxDevToolsPostProcessor_Begin, "Begin Post Processing");

      if (aRequest is not IReduxRequest)
      {
        await ReduxDevToolsInterop.DispatchAsync(aRequest, Store.GetSerializableState());
      }
      Logger.LogDebug(EventIds.ReduxDevToolsPostProcessor_End, "Post Processing Completed");
    }
    catch (Exception aException)
    {
      Logger.LogDebug
      (
        EventIds.ReduxDevToolsPostProcessor_Exception,
        aException,
        "Error dispatching Request to Redux DevTools"
      );

      throw;
    }
  }
}
