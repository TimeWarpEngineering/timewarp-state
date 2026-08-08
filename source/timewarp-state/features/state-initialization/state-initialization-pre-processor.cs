namespace TimeWarp.State;

public sealed class StateInitializationPreProcessor<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
  where TMessage : IAction
{
  private readonly IStore Store;
  private readonly ILogger<StateInitializationPreProcessor<TMessage, TResponse>> Logger;

  public StateInitializationPreProcessor(IStore store, ILogger<StateInitializationPreProcessor<TMessage, TResponse>> logger)
  {
    Store = store;
    Logger = logger;
  }

  protected override async ValueTask Handle(TMessage message, CancellationToken cancellationToken)
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
  }
}
