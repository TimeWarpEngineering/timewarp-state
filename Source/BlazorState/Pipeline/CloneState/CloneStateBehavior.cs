namespace BlazorState.Pipeline.State;

using AnyClone;
using BlazorState;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CloneStateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger Logger;
  private readonly IMediator Mediator;
  private readonly IStore Store;
  private bool IsClientSide;

  public CloneStateBehavior
  (
    ILogger<CloneStateBehavior<TRequest, TResponse>> aLogger,
    IStore aStore,
    IMediator aMediator
  )
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.CloneStateBehavior_Initializing, "constructing");
    Store = aStore;
    Mediator = aMediator;
  }

  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNext,
    CancellationToken aCancellationToken
  )
  {
    Type declaringType = typeof(TRequest).DeclaringType;

    IState originalState = default;
    // Constrain here if not IState then ignore.
    if (typeof(IState).IsAssignableFrom(declaringType))
    {
      IsClientSide = true;
      originalState = Store.GetState(declaringType) as IState;
      IState newState = (originalState is ICloneable clonable) ? (IState)clonable.Clone() : originalState.Clone();
      Logger.LogDebug
      (
        EventIds.CloneStateBehavior_Cloning,
        "Clone State of type {declaringType} originalState.Guid:{originalState_Guid} newState.Guid:{newState_Guid}",
        declaringType,
        originalState?.Guid,
        newState.Guid
      );

      Store.SetState(newState as IState);
    }
    else
    {
      Logger.LogDebug
      (
        EventIds.CloneStateBehavior_Ignoring,
        "Not cloning State because {declaringType} is not an IState",
        declaringType
      );
    }

    try
    {
      TResponse response = await aNext();
      return response;
    }
    catch (Exception aException)
    {
      // If something fails we restore system to previous state.
      Logger.LogWarning(EventIds.CloneStateBehavior_Exception, aException, "Error cloning State");

      if (IsClientSide && originalState != null)
      {
        Store.SetState(originalState);

        Logger.LogWarning
        (
          EventIds.CloneStateBehavior_Restored,
          "Restored State of type: {declaringType}",
          declaringType
        );

        var exceptionNotification = new ExceptionNotification
        {
          RequestName = nameof(CloneStateBehavior<TRequest, TResponse>),
          Exception = aException
        };

        await Mediator.Publish(exceptionNotification);
        return default;
      }

      throw;
    }
  }
}
