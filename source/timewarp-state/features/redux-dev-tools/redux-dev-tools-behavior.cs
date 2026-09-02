namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Pipeline behavior that forwards each handled action and the resulting state to the Redux DevTools
/// browser extension.
/// </summary>
/// <remarks>
/// Woven at compile time by <c>[assembly: MediatorBehavior]</c> (assembly-marker.cs), so it is present in
/// every host pipeline. It is active only when <see cref="ServiceCollectionExtensions.UseReduxDevTools"/>
/// registered <see cref="ReduxDevToolsOptions"/> (and its interop/store services); otherwise the
/// optional dependencies resolve to <c>null</c> and the behavior is a pass-through.
/// </remarks>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ReduxDevToolsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger;
  private readonly ReduxDevToolsInterop? ReduxDevToolsInterop;
  private readonly ReduxDevToolsOptions? ReduxDevToolsOptions;
  private readonly IReduxDevToolsStore? Store;
  private readonly Regex? TraceFilterRegex;

  public ReduxDevToolsBehavior
  (
    ILogger<ReduxDevToolsBehavior<TRequest, TResponse>> logger,
    ReduxDevToolsInterop? reduxDevToolsInterop = null,
    ReduxDevToolsOptions? reduxDevToolsOptions = null,
    IReduxDevToolsStore? store = null
  )
  {
    Logger = logger;
    Store = store;
    ReduxDevToolsInterop = reduxDevToolsInterop;
    ReduxDevToolsOptions = reduxDevToolsOptions;
    TraceFilterRegex = ReduxDevToolsOptions is null ? null : new Regex(ReduxDevToolsOptions.TraceFilterExpression);
    
    string className = typeof(ReduxDevToolsBehavior<,>).Name.Split('`')[0];
    Logger.LogDebug
    (
      EventIds.StateTransactionBehavior_Constructing,
      "constructing {ClassName}<{RequestType},{ResponseType}>",
      className, 
      typeof(TRequest).Name,
      typeof(TResponse).Name
    );
  }

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    // UseReduxDevTools was not called: nothing to dispatch to.
    if (ReduxDevToolsOptions is null || ReduxDevToolsInterop is null || Store is null || TraceFilterRegex is null)
    {
      return await next(cancellationToken);
    }

    Logger.LogDebug(EventIds.ReduxDevToolsBehavior_Begin, "{classname}: Start", GetType().Name);

    string? stackTrace = null;
    int maxItems = ReduxDevToolsOptions.TraceLimit == 0 ? int.MaxValue : ReduxDevToolsOptions.TraceLimit;

    if (ReduxDevToolsOptions.Trace) stackTrace = BuildStackTrace(maxItems, TraceFilterRegex);
    TResponse response = await next(cancellationToken);

    try
    {
      await ReduxDevToolsInterop.DispatchAsync(request, Store.GetSerializableState(), stackTrace);
      Logger.LogDebug(EventIds.ReduxDevToolsBehavior_End, "ReduxDevToolsBehavior Completed");
    }
    catch (Exception exception)
    {
      Logger.LogDebug
      (
        EventIds.ReduxDevToolsBehavior_Exception,
        exception,
        "Error dispatching Request to Redux DevTools"
      );

      throw;
    }

    return response;
  }

  private static string BuildStackTrace(int maxItems, Regex traceFilterRegex)
  {
    StringBuilder stringBuilder = new();
    return string.Join
    (
      "\r\n",
      new StackTrace(fNeedFileInfo: true)
        .GetFrames()
        .Select
        (
          stackFrame =>
          {
            stringBuilder.Clear();
            stringBuilder.Append("at ");
            stringBuilder.Append(stackFrame.GetMethod()?.DeclaringType?.FullName);
            stringBuilder.Append('.');
            stringBuilder.Append(stackFrame.GetMethod()?.Name);
            stringBuilder.Append(' ');
            if (stackFrame.GetFileName() is not null)
            {
              stringBuilder.Append('(');
              stringBuilder.Append(stackFrame.GetFileName());
              stringBuilder.Append(':');
              stringBuilder.Append(stackFrame.GetFileLineNumber());
              stringBuilder.Append(':');
              stringBuilder.Append(stackFrame.GetFileColumnNumber());
              stringBuilder.Append(')');
            }
            return stringBuilder.ToString();
          }
        )
        .Where(x => traceFilterRegex.IsMatch(x))
        .Take(maxItems)
    );
  }
}
