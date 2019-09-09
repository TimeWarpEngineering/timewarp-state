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

    private readonly ILogger Logger;
    private readonly IStore Store;

    public async Task<TResponse> Handle(
      TRequest aRequest,
      CancellationToken aCancellationToken,
      RequestHandlerDelegate<TResponse> aNext)
    {
      Type declaringType = typeof(TRequest).DeclaringType;
      // logging variables
      string className = GetType().Name;
      className = className.Remove(className.IndexOf('`'));
      string requestId = $"{aRequest.GetType().FullName}:{aRequest.GetHashCode()}";

      Logger.LogDebug($"Pipeline Start: {requestId}");
      Logger.LogDebug($"{className}: Start");

      IState originalState = default;
      // Constrain here if not IState then ignore.
      if (typeof(IState).IsAssignableFrom(declaringType))
      {
        Logger.LogDebug($"{className}: Clone State of type {declaringType}");
        originalState = Store.GetState(declaringType) as IState;
        Logger.LogDebug($"{className}: originalState.Guid:{originalState.Guid}");
        IState newState = (originalState is ICloneable clonable) ? (IState)clonable.Clone() : originalState.Clone();
        Logger.LogDebug($"{className}: newState.Guid:{newState.Guid}");
        Store.SetState(newState as IState);
      }
      else
      {
        Logger.LogDebug($"{className}: Not cloning State because {declaringType} is not an IState");
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
        Logger.LogError($"{className}: Restoring State of type: {declaringType}");
        if (originalState != null)
        {
          Store.SetState(originalState as IState);
        }

        throw;  // Do you throw or not? for now yes.
      }
    }
  }
}