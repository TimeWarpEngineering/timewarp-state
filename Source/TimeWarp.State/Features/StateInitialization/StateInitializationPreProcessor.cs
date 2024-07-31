namespace TimeWarp.State;

public class StateInitializationPreProcessor<TRequest> : IRequestPreProcessor<TRequest> where TRequest : IAction
{
  private readonly IStore Store;
  private readonly ILogger<StateInitializationPreProcessor<TRequest>> Logger;

  public StateInitializationPreProcessor(IStore store, ILogger<StateInitializationPreProcessor<TRequest>> logger)
  {
    Store = store;
    Logger = logger;
  }

  public async Task Process(TRequest request, CancellationToken cancellationToken)
  {
    string typeName = typeof(TRequest).GetEnclosingStateType().FullName ?? throw new InvalidOperationException();

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
