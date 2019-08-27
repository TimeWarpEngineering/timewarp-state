namespace BlazorState.Pipeline.State
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.Logging;
  using AnyClone;

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

      var originalState = default(TResponse);
      // Constrain here if not IState then ignore.
      if (typeof(IState).IsAssignableFrom(responseType))
      {
        Logger.LogDebug($"{className}: Clone State of type {responseType}");
        originalState = Store.GetState<TResponse>();
        Logger.LogDebug($"{className}: originalState.Guid:{((IState)originalState).Guid}");
        TResponse newState = (originalState is ICloneable clonable) ? (TResponse)clonable.Clone() : originalState.Clone();
        Logger.LogDebug($"{className}: newState.Guid:{((IState)newState).Guid}");
        Store.SetState(newState as IState);
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
        // One may consider extension point here for error handling.
        // Maybe if error occurs on one action we want to launch another action to
        // Update some error state so the user knows of the failure.
        // But as a rule if this is an exception it should be unexpected.
        Logger.LogError($"{className}: Error: {aException.Message}");
        Logger.LogError($"{className}: InnerError: {aException?.InnerException?.Message}");
        Logger.LogError($"{className}: Restoring State of type: {responseType}");
        if (originalState != null)
        {
          Store.SetState(originalState as IState);
        }

        throw;  // Do you throw or not? for now yes.
      }
    }
  }
}