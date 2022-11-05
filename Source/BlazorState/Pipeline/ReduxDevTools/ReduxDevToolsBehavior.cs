#nullable enable
namespace BlazorState.Pipeline.ReduxDevTools;

using BlazorState;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ReduxDevToolsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger Logger;
  private readonly ReduxDevToolsInterop ReduxDevToolsInterop;
  private readonly ReduxDevToolsOptions ReduxDevToolsOptions;
  private readonly IReduxDevToolsStore Store;
  private readonly Regex TraceFilterRegex;

  public ReduxDevToolsBehavior
  (
    ILogger<ReduxDevToolsBehavior<TRequest, TResponse>> aLogger,
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

  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNext,
    CancellationToken aCancellationToken
  )
  {
    Logger.LogDebug("{classname}: Start", GetType().Name);

    string? stackTrace = null;
    int maxItems = ReduxDevToolsOptions.TraceLimit == 0 ? int.MaxValue : ReduxDevToolsOptions.TraceLimit;
    StringBuilder stringBuilder = new();
    if (ReduxDevToolsOptions.Trace)
      stackTrace =
        string.Join
        (
          "\r\n",
          new StackTrace(fNeedFileInfo: true)
            .GetFrames()
            .Select
            (
              aStackFrame =>
              {
                stringBuilder.Clear();
                stringBuilder.Append("at ");
                stringBuilder.Append(aStackFrame.GetMethod()?.DeclaringType?.FullName);
                stringBuilder.Append('.');
                stringBuilder.Append(aStackFrame.GetMethod()?.Name);
                stringBuilder.Append(' ');
                if (aStackFrame.GetFileName() is not null)
                {
                  stringBuilder.Append('(');
                  stringBuilder.Append(aStackFrame.GetFileName());
                  stringBuilder.Append(':');
                  stringBuilder.Append(aStackFrame.GetFileLineNumber());
                  stringBuilder.Append(':');
                  stringBuilder.Append(aStackFrame.GetFileColumnNumber());
                  stringBuilder.Append(')');
                }
                return stringBuilder.ToString();                
              }
            )
            .Where(x => TraceFilterRegex?.IsMatch(x) != false)
            .Take(maxItems)
        );

    Logger.LogDebug("{classname}: Call next", GetType().Name);
    TResponse response = await aNext();

    try
    {
      Logger.LogDebug("{classname}: Start", GetType().Name);
      if (aRequest is not IReduxRequest)
      {
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
    return response;
  }
}
