namespace BlazorState.Pipeline.ReduxDevTools;

using BlazorState;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
  private readonly ReduxDevToolsOptions ReduxDevToolsOptions;
  private readonly IReduxDevToolsStore Store;
  private readonly Regex TraceFilterRegex;

  public ReduxDevToolsPostProcessor
  (
    ILogger<ReduxDevToolsPostProcessor<TRequest, TResponse>> aLogger,
    ReduxDevToolsInterop aReduxDevToolsInterop,
    ReduxDevToolsOptions aReduxDevToolsOptions,
    IReduxDevToolsStore aStore
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.ReduxDevToolsPostProcessor_Constructing, "constructing ReduxDevToolsPostProcessor");
    Store = aStore;
    ReduxDevToolsInterop = aReduxDevToolsInterop;
    ReduxDevToolsOptions = aReduxDevToolsOptions;
    TraceFilterRegex = new Regex(aReduxDevToolsOptions.TraceFilterExpression);
  }

  public async Task Process(TRequest aRequest, TResponse aResponse, CancellationToken aCancellationToken)
  {
    try
    {
      Logger.LogDebug(EventIds.ReduxDevToolsPostProcessor_Begin, "Begin Post Processing");

      if (aRequest is not IReduxRequest)
      {
        string fakeStack = "YoYo\nat someplace (filename:10:11)";
        string stackTrace = fakeStack;
        int maxItems = ReduxDevToolsOptions.TraceLimit == 0 ? int.MaxValue : ReduxDevToolsOptions.TraceLimit;
        if (ReduxDevToolsOptions.Trace)
          stackTrace =
            string.Join("\r\n",
              new StackTrace(fNeedFileInfo: true)
                .GetFrames()
                .Select(x => $"at {x.GetMethod().DeclaringType.FullName}.{x.GetMethod().Name} ({x.GetFileName()}:{x.GetFileLineNumber()}:{x.GetFileColumnNumber()})")
                .Where(x => TraceFilterRegex?.IsMatch(x) != false)
                .Take(maxItems));

        await ReduxDevToolsInterop.DispatchAsync(aRequest, Store.GetSerializableState(), stackTrace);
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
