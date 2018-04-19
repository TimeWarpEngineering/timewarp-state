namespace BlazorState.Behaviors.State
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.State;
  using BlazorState.Store;
  using MediatR;
  using Microsoft.Extensions.Logging;

  public class CloneStateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
      TRequest request,
      CancellationToken cancellationToken,
      RequestHandlerDelegate<TResponse> next)
    {
      Logger.LogDebug($"Pipeline Start: {request.GetType().FullName}");
      Logger.LogDebug($"{GetType().Name}: Start");

      Type responseType = typeof(TResponse);

      IState originalState = null;
      // Constrain here if not IState then ignore.
      if (typeof(IState).IsAssignableFrom(responseType))
      {
        Logger.LogDebug($"{GetType().Name}: Clone State of type {responseType}");
        originalState = (IState)Store.GetState<TResponse>();
        Logger.LogDebug($"{GetType().Name}: originalState.Guid:{originalState.Guid}");
        var newState = (IState)originalState.Clone();
        Logger.LogDebug($"{GetType().Name}: newState.Guid:{newState.Guid}");

        Store.SetState(newState);
      }
      else
      {
        Logger.LogDebug($"{GetType().Name}: Not cloning State because {responseType.Name} is not an IState");
      }
      try
      {
        Logger.LogDebug($"{GetType().Name}: {GetType().Name}: Call next");
        TResponse response = await next();
        Logger.LogDebug($"{GetType().Name}: {GetType().Name}: Start Post Processing");
        Logger.LogDebug($"{GetType().Name}: {GetType().Name}: End");
        return response;
      }
      catch (Exception aException)
      {
        // If something fails we restore system to previous state.
        // One may consider extention point here for error handling.
        // Maybe if error occurs on one action we want to launch another action to
        // Update some error state so the user knows of the failure.
        // But as a rule if this is an exception it should be unexpected.
        Logger.LogDebug($"{GetType().Name}: Error: {aException.Message}");
        Logger.LogDebug($"{GetType().Name}: InnerError: {aException?.InnerException.Message}");
        Logger.LogDebug($"{GetType().Name}: Restoring State of type: {responseType}");
        if (originalState != null)
          Store.SetState(originalState);
        throw;  // Do you throw or not? for now yes.
      }
    }
  }
}