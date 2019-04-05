namespace BlazorState.Behaviors.ReduxDevTools
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorState.Behaviors.ReduxDevTools.Features;
  using MediatR;
  using Microsoft.Extensions.Logging;

  //TODO: this should be a IRequestPostProcessor but I couldn't get it to work.

  /// <summary>
  ///
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  internal class ReduxDevToolsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  //public class ReduxDevToolsBehavior<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
  {
    public ReduxDevToolsBehavior(
      ILogger<ReduxDevToolsBehavior<TRequest, TResponse>> aLogger,
      ReduxDevToolsInterop aReduxDevToolsInterop,
      IReduxDevToolsStore aStore)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name} constructor");
      Store = aStore;
      ReduxDevToolsInterop = aReduxDevToolsInterop;
    }

    private ILogger Logger { get; }
    private ReduxDevToolsInterop ReduxDevToolsInterop { get; }

    //private History<TState> History { get; }
    private IReduxDevToolsStore Store { get; }

    public async Task<TResponse> Handle(
      TRequest aRequest,
      CancellationToken aCancellationToken,
      RequestHandlerDelegate<TResponse> aNext)
    {
      Logger.LogDebug($"{GetType().Name}: Start");
      Logger.LogDebug($"{GetType().Name}: Call next");
      TResponse response = await aNext();
      Logger.LogDebug($"{GetType().Name}: Start Post Processing");
      try
      {
        if (!(aRequest is IReduxRequest) && ReduxDevToolsInterop.IsEnabled)
        {
          ReduxDevToolsInterop.Dispatch(aRequest, Store.GetSerializableState());
        }
        Logger.LogDebug($"{GetType().Name}: End");
        return response;
      }
      catch (Exception e)
      {
        Logger.LogDebug($"{GetType().Name}: Error: {e.Message}");
        Logger.LogDebug($"{GetType().Name}: InnerException: {e.InnerException?.Message}");
        Logger.LogDebug($"{GetType().Name}: StackTrace: {e.StackTrace}");
        throw;
      }
    }

    //TODO: This won't run as a PostProcessor for some reason MediatR never creates it.
    public Task Process(TRequest aRequest, TResponse aResponse)
    {
      try
      {
        Logger.LogDebug($"{GetType().Name}: Start Post Processing");
        if (!(aRequest is IReduxRequest))
        {
          ReduxDevToolsInterop.Dispatch(aRequest, Store.GetSerializableState());
        }
        Logger.LogDebug($"{GetType().Name}: End");
      }
      catch (Exception e)
      {
        Logger.LogDebug($"{GetType().Name}: Error: {e.Message}");
        Logger.LogDebug($"{GetType().Name}: InnerException: {e.InnerException?.Message}");
        Logger.LogDebug($"{GetType().Name}: StackTrace: {e.StackTrace}");
        throw;
      }
      return Task.CompletedTask;
    }
  }
}