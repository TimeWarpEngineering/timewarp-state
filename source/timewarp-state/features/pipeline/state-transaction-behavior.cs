#region Purpose
// Clone state before the handler runs; on failure, restore that clone only when it is the live state.
#endregion

#region Design
// Clone is outside the try; the catch is a handler failure, not a clone failure.
// ExceptionNotification is published with CancellationToken.None so a cancelled request token cannot skip reporting.
// OperationCanceledException rolls back when this action's clone is live, is not published, and is rethrown.
// Other handler exceptions are published and then rethrown only when TimeWarpStateOptions.RethrowHandlerExceptions is set.
// Otherwise Send returns the default response after rollback.
// Rollback uses ReferenceEquals against the clone this action installed. A different instance is a later action's
// committed clone, so the snapshot stays out of the store and the skip is logged. Actions are not serialized: a
// per-state lock can deadlock a handler that waits on work which re-enters that state, and WaitAsync does not
// remove that deadlock. In-place writes on the live clone are not a separate commit.
#endregion

namespace TimeWarp.Features.StateTransactions;

/// <summary>
///   Represents a pipeline behavior in TimeWarp.State that clones the current state before processing a request.
///   This behavior ensures that the state can be reverted to its original form in case of an error during the request handling,
///   unless a later action has already committed a newer clone of that state.
///   A state that implements <see cref="ICloneable"/> is cloned with that method. Every other state is cloned with the
///   delegate the clone source generator registered in <see cref="TimeWarp.Features.Cloning.StateCloneRegistry"/>.
///   The clone does not block, so it stays safe on single-threaded browser WebAssembly. This behavior is
///   critical for maintaining application consistency and enables undo functionality.
/// </summary>
/// <remarks>
///   This behavior is part of the TimeWarp.State pipeline, intercepting actions (requests) to clone the relevant state before
///   proceeding. If an action fails or is cancelled, and the store holds this action's clone, the behavior restores the
///   pre-action state. When the store holds a different instance, a concurrent action has committed a newer clone, the
///   behavior leaves that state in place, and it logs the skipped rollback. It uses TimeWarp.Mediator's pipeline behavior feature to hook into the request handling
///   process.
/// </remarks>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public sealed class StateTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger;
  private readonly IPublisher<ClientPipeline> Publisher;
  private readonly IStore Store;
  private readonly bool Enabled;
  private readonly bool RethrowHandlerExceptions;

  public StateTransactionBehavior
  (
    ILogger<StateTransactionBehavior<TRequest, TResponse>> logger,
    IStore store,
    IPublisher<ClientPipeline> publisher,
    TimeWarpStateOptions timeWarpStateOptions
  )
  {
    Logger = logger;
    Store = store;
    Publisher = publisher;
    // The behavior is woven at compile time; TimeWarpStateOptions.UseStateTransactionBehavior turns it off at runtime.
    Enabled = timeWarpStateOptions.UseStateTransactionBehavior;
    RethrowHandlerExceptions = timeWarpStateOptions.RethrowHandlerExceptions;

    string className = typeof(StateTransactionBehavior<,>).GetSimpleName();

    Logger.LogDebug
    (
      EventIds.StateTransactionBehavior_Constructing,
      message: "constructing {ClassName}<{RequestType},{ResponseType}>",
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
    if (!Enabled) return await next(cancellationToken);

    // Analyzer will ensure the following.  If IAction it has to be nested in a IState implementation.
    Type enclosingStateType = typeof(TRequest).GetEnclosingStateType();
    IState originalState = (IState)Store.GetState(enclosingStateType);
    IState newState = originalState is ICloneable cloneable
      ? (IState)cloneable.Clone()
      : StateCloneRegistry.Clone(originalState);

    // We don't clone the Sender, it is an injected service and not part of state.
    newState.Sender = originalState.Sender;

    if (newState.Guid == Guid.Empty)
    {
      throw new InvalidCloneException(enclosingStateType, InvalidCloneException.Cause.EmptyGuid);
    }

    if (originalState.Guid == newState.Guid)
    {
      throw new InvalidCloneException(enclosingStateType, InvalidCloneException.Cause.EqualGuid);
    }

    Logger.LogDebug
    (
      EventIds.StateTransactionBehavior_Cloning,
      message: "Cloned State of type {declaringType} originalState.Guid:{originalState_Guid} newState.Guid:{newState_Guid}",
      enclosingStateType,
      originalState.Guid,
      newState.Guid
    );

    Store.SetState(newState);

    try
    {
      TResponse response = await next(cancellationToken);
      return response;
    }
    catch (Exception exception)
    {
      bool isCancellation = exception is OperationCanceledException;

      if (!isCancellation)
      {
        Logger.LogWarning
        (
          EventIds.StateTransactionBehavior_Exception,
          exception,
          message: "Error handling action. Type:{enclosingStateType}",
          enclosingStateType
        );
      }

      // A newer clone belongs to a concurrent action. Restoring this snapshot would discard that commit.
      if (ReferenceEquals(Store.GetState(enclosingStateType), newState))
      {
        Logger.LogInformation
        (
          EventIds.StateTransactionBehavior_Restoring,
          message: "Attempting to restore State of type: {enclosingStateType}",
          enclosingStateType
        );

        Store.SetState(originalState);
      }
      else
      {
        Logger.LogWarning
        (
          EventIds.StateTransactionBehavior_ConcurrentAdvance,
          message: "Skipping rollback because a concurrent action advanced the state. Type:{enclosingStateType}",
          enclosingStateType
        );
      }

      if (!isCancellation)
      {
        ExceptionNotification exceptionNotification = new
        (
          requestName: nameof(StateTransactionBehavior<TRequest, TResponse>),
          exception: exception
        );

        await Publisher.Publish(exceptionNotification, CancellationToken.None);
      }

      // Cancellation must propagate so callers stop chaining work. Handler exceptions propagate only
      // when the host opts in; the default keeps Send from faulting after ExceptionNotification.
      if (isCancellation || RethrowHandlerExceptions)
      {
        throw;
      }

      return default!;// It can be null, but we don't care since TimeWarp.Mediator handles null values gracefully.
    }
  }
}
