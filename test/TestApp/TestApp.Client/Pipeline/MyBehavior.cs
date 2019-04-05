namespace TestApp.Client.Behaviors
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Sample Pipeline Behavior AKA Middle-ware
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  /// <remarks>see MediatR for more examples</remarks>
  public class MyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  {
    public MyBehavior(
      ILogger<MyBehavior<TRequest, TResponse>> aLogger,
      IStore aStore)
    {
      Logger = aLogger;
      Store = aStore;
      Logger.LogDebug($"{GetType().Name}: Constructor");
    }

    public Guid Guid { get; } = Guid.NewGuid();
    private ILogger Logger { get; }
    private IStore Store { get; }

    public async Task<TResponse> Handle(
      TRequest aRequest,
      CancellationToken aCancellationToken,
      RequestHandlerDelegate<TResponse> aNext)
    {
      Logger.LogDebug($"{GetType().Name}: Start");

      Logger.LogDebug($"{GetType().Name}: Call next");
      TResponse newState = await aNext();
      Logger.LogDebug($"{GetType().Name}: Start Post Processing");
      // Constrain here based on a type or anything you want.
      if (typeof(IState).IsAssignableFrom(typeof(TResponse)))
      {
        Logger.LogDebug($"{GetType().Name}: Do Constrained Action");
      }

      Logger.LogDebug($"{GetType().Name}: End");
      return newState;
    }
  }
}