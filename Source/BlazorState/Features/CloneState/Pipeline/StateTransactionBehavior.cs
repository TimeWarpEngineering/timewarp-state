#nullable enable
namespace TimeWarp.Features.StateTransactions;

/// <summary>
///   Represents a pipeline behavior in BlazorState that clones the current state before processing a request.
///   This behavior ensures that the state can be reverted to its original form in case of an error during the request handling.
///   The cloning process is contingent upon the state implementing <see cref="ICloneable"/>, allowing for a deep copy.
///   If the state does not implement <see cref="ICloneable"/>, it falls back to a custom clone method. This behavior is
///   critical for maintaining application consistency and enables undo functionality.
/// </summary>
/// <remarks>
///   This behavior is part of the BlazorState pipeline, intercepting actions (requests) to clone the relevant state before
///   proceeding. If an action fails, the system reverts to the cloned state, thus preventing partial state updates
///   from corrupting the application state. It uses MediatR's pipeline behavior feature to hook into the request handling
///   process.
/// </remarks> 
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public sealed class StateTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IAction
{
  private readonly ILogger Logger;
  private readonly IPublisher Publisher;
  private readonly IStore Store;

  public StateTransactionBehavior
  (
    ILogger<StateTransactionBehavior<TRequest, TResponse>> logger,
    IStore store,
    IPublisher publisher
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.StateTransactionBehavior_Initializing, "constructing");
    Store = store;
    Publisher = publisher;
  }

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    // Analyzer will ensure the following.  If IAction it has to be nested in a IState implementation.
    Type enclosingStateType = typeof(TRequest).GetEnclosingStateType();
    var originalState = (IState)Store.GetState(enclosingStateType)!; // Not null because of Analyzer
    IState newState = 
      (originalState is ICloneable cloneable) ? 
      (IState)cloneable.Clone() : 
      originalState.Clone();

    if (newState.Guid == Guid.Empty || originalState.Guid == newState.Guid)
    {
      throw new InvalidCloneException(enclosingStateType);
    }

    Logger.LogDebug
    (
      EventIds.StateTransactionBehavior_Cloning,
      "Clone State of type {declaringType} originalState.Guid:{originalState_Guid} newState.Guid:{newState_Guid}",
      enclosingStateType,
      originalState.Guid,
      newState.Guid
    );

    Store.SetState(newState);

    try
    {
      TResponse response = await next();
      return response;
    }
    catch (Exception exception)
    {
      // If something fails we restore system to previous state.
      Logger.LogWarning(EventIds.StateTransactionBehavior_Exception, exception, "Error cloning State");

      Store.SetState(originalState);

      Logger.LogWarning
      (
        EventIds.StateTransactionBehavior_Restored,
        "Restored State of type: {enclosingStateType}",
        enclosingStateType
      );

      var exceptionNotification = new ExceptionNotification
      (
        requestName: nameof(StateTransactionBehavior<TRequest, TResponse>),
        exception: exception
      );

      await Publisher.Publish(exceptionNotification, cancellationToken);
      return default!; // It can be null, but we don't care since MediatR handles null values gracefully.
    }
  }
}
