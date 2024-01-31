#nullable enable
namespace BlazorState.Pipeline.State;

internal sealed class CloneStateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger;
  private readonly IPublisher Publisher;
  private readonly IStore Store;

  public CloneStateBehavior
  (
    ILogger<CloneStateBehavior<TRequest, TResponse>> logger,
    IStore store,
    IPublisher publisher
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.CloneStateBehavior_Initializing, "constructing");
    Store = store;
    Publisher = publisher;
  }

  public async Task<TResponse> Handle
  (
    TRequest aRequest,
    RequestHandlerDelegate<TResponse> aNext,
    CancellationToken aCancellationToken
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
      EventIds.CloneStateBehavior_Cloning,
      "Clone State of type {declaringType} originalState.Guid:{originalState_Guid} newState.Guid:{newState_Guid}",
      enclosingStateType,
      originalState.Guid,
      newState.Guid
    );

    Store.SetState(newState);

    try
    {
      TResponse response = await aNext();
      return response;
    }
    catch (Exception exception)
    {
      // If something fails we restore system to previous state.
      Logger.LogWarning(EventIds.CloneStateBehavior_Exception, exception, "Error cloning State");

      Store.SetState(originalState);

      Logger.LogWarning
      (
        EventIds.CloneStateBehavior_Restored,
        "Restored State of type: {enclosingStateType}",
        enclosingStateType
      );

      var exceptionNotification = new ExceptionNotification
      {
        RequestName = nameof(CloneStateBehavior<TRequest, TResponse>),
        Exception = exception
      };

      await Publisher.Publish(exceptionNotification, aCancellationToken);
      return default!; // It can be null, but we don't care since MediatR handles null values gracefully.
    }
  }
}
