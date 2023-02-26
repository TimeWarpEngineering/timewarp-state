namespace TestApp.Client.Pipeline;

using BlazorState;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Sample Pipeline Behavior AKA Middle-ware
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <remarks>see MediatR for more examples</remarks>
public class MyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  private readonly ILogger Logger;
  public Guid Guid { get; } = Guid.NewGuid();

  public MyBehavior
  (
    ILogger<MyBehavior<TRequest, TResponse>> aLogger
  )
  {
    Logger = aLogger;
    Logger.LogDebug("{classname}: Constructor", GetType().Name);
  }

  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNext,
    CancellationToken aCancellationToken
  )
  {
    Logger.LogDebug("{classname}: Start", GetType().Name);

    Logger.LogDebug("{classname}: Call next", GetType().Name);
    TResponse newState = await aNext();
    Logger.LogDebug("{classname}: Start Post Processing", GetType().Name);
    // Constrain here based on a type or anything you want.
    if (typeof(IState).IsAssignableFrom(typeof(TResponse)))
    {
      Logger.LogDebug("{classname}: Do Constrained Action", GetType().Name);
    }

    Logger.LogDebug("{classname}: End", GetType().Name);
    return newState;
  }
}
