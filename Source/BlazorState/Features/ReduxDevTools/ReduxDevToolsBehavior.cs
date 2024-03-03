#nullable enable
namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
///
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ReduxDevToolsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
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
    Logger.LogDebug(EventIds.ReduxDevToolsBehavior_Constructing, "constructing ReduxDevToolsBehavior");
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
    Logger.LogDebug(EventIds.ReduxDevToolsBehavior_Begin,"{classname}: Start", GetType().Name);

    string? stackTrace = null;
    int maxItems = ReduxDevToolsOptions.TraceLimit == 0 ? int.MaxValue : ReduxDevToolsOptions.TraceLimit;
    
    if (ReduxDevToolsOptions.Trace) stackTrace = BuildStackTrace(maxItems);

    Logger.LogDebug("{classname}: Call next", GetType().Name);
    TResponse response = await aNext();

    try
    {
      Logger.LogDebug("{classname}: Start", GetType().Name);
      await ReduxDevToolsInterop.DispatchAsync(aRequest, Store.GetSerializableState(), stackTrace);
      Logger.LogDebug(EventIds.ReduxDevToolsBehavior_End, "ReduxDevToolsBehavior Completed");
    }
    catch (Exception aException)
    {
      Logger.LogDebug
      (
        EventIds.ReduxDevToolsBehavior_Exception,
        aException,
        "Error dispatching Request to Redux DevTools"
      );

      throw;
    }
    return response;
  }

  private string BuildStackTrace(int maxItems)
  {
    StringBuilder stringBuilder = new();
    return string.Join
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
  }
}
