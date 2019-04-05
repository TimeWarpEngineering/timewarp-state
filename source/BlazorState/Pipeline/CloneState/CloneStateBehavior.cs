namespace BlazorState.Behaviors.State
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;

  internal class CloneStateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  {
    public CloneStateBehavior(
      ILogger<CloneStateBehavior<TRequest, TResponse>> aLogger,
      IStore aStore)
    {
      Logger = aLogger;
      Logger.LogDebug($"{GetType().Name} constructor");
      Store = aStore;
    }

    private ILogger Logger { get; }
    private IStore Store { get; }

    public async Task<TResponse> Handle(
      TRequest aRequest,
      CancellationToken aCancellationToken,
      RequestHandlerDelegate<TResponse> aNext)
    {
      // logging variables
      string className = GetType().Name;
      className = className.Remove(className.IndexOf('`'));
      string requestId = $"{aRequest.GetType().FullName}:{aRequest.GetHashCode()}";

      Logger.LogDebug($"Pipeline Start: {requestId}");
      Logger.LogDebug($"{className}: Start");

      Type responseType = typeof(TResponse);

      IState originalState = null;
      // Constrain here if not IState then ignore.
      if (typeof(IState).IsAssignableFrom(responseType))
      {
        Logger.LogDebug($"{className}: Clone State of type {responseType}");
        originalState = (IState)Store.GetState<TResponse>();
        Logger.LogDebug($"{className}: originalState.Guid:{originalState.Guid}");
        var newState = (IState)originalState.Clone();
        Logger.LogDebug($"{className}: newState.Guid:{newState.Guid}");

        Store.SetState(newState);
      }
      else
      {
        Logger.LogDebug($"{className}: Not cloning State because {responseType.Name} is not an IState");
      }
      try
      {
        Logger.LogDebug($"{className}: Call next");
        TResponse response = await aNext();
        Logger.LogDebug($"{className}: Start Post Processing");
        Logger.LogDebug($"{className}: End Post Processing");
        Logger.LogDebug($"Pipeline End: {requestId}");
        return response;
      }
      catch (Exception aException)
      {
        // If something fails we restore system to previous state.
        // One may consider extention point here for error handling.
        // Maybe if error occurs on one action we want to launch another action to
        // Update some error state so the user knows of the failure.
        // But as a rule if this is an exception it should be unexpected.
        Logger.LogError($"{className}: Error: {aException.Message}");
        Logger.LogError($"{className}: InnerError: {aException?.InnerException?.Message}");
        Logger.LogError($"{className}: Restoring State of type: {responseType}");
        if (originalState != null)
          Store.SetState(originalState);
        throw;  // Do you throw or not? for now yes.
      }
    }
  }
}