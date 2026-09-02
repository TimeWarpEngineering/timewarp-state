namespace TimeWarp.State;

/// <summary>
/// Pipeline behavior that waits for the enclosing state's initialization (persistence load, etc.)
/// to complete before an action is handled. Woven by <c>[assembly: MediatorBehavior]</c> in
/// assembly-marker.cs; closes only onto <see cref="IAction"/> requests.
/// </summary>
public sealed class StateInitializationPreProcessor<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
  where TMessage : notnull, IAction
{
  private readonly IStore Store;
  private readonly ILogger<StateInitializationPreProcessor<TMessage, TResponse>> Logger;

  public StateInitializationPreProcessor(IStore store, ILogger<StateInitializationPreProcessor<TMessage, TResponse>> logger)
  {
    Store = store;
    Logger = logger;
  }

  public async Task<TResponse> Handle
  (
    TMessage message,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    string typeName = typeof(TMessage).GetEnclosingStateType().FullName ?? throw new InvalidOperationException();

    // Wait for the state initialization to complete before processing the action
    if (Store.StateInitializationTasks.TryGetValue(typeName, out Task? initializationTask))
    {
      try
      {
        Logger.LogTrace
        (
          EventIds.StateInitializationPreProcessor_Waiting,
          "Waiting for state initialization to complete. State type: {StateType}",
          typeName
        );

        await initializationTask;

        Logger.LogTrace
        (
          EventIds.StateInitializationPreProcessor_Completed,
          "State initialization completed. State type: {StateType}",
          typeName
        );
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "Error occurred while waiting for state initialization.");
        throw;
      }
    }

    return await next(cancellationToken);
  }
}
