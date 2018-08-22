namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// </summary>
  /// <remarks>TODO: A significant amount of this class is to support
  /// Redux dev tools.  Should we not split this up so that portion
  /// is only included if in dev mode.
  /// </remarks>
  internal partial class Store : IStore
  {
    public Store(
      IServiceProvider aServiceProvider
      , ILogger<Store> aLogger
    )
    {
      Logger = aLogger;
      using (Logger.BeginScope(nameof(Store)))
      {
        Logger.LogInformation($"{GetType().Name}: constructor: {nameof(Guid)}:{Guid}");
        ServiceProvider = aServiceProvider;
        States = new Dictionary<string, IState>();
      }
    }

    /// <summary>
    /// Unique Guid for the Store.
    /// </summary>
    /// <remarks>Useful when logging </remarks>
    public Guid Guid { get; } = Guid.NewGuid();
    private ILogger Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IDictionary<string, IState> States { get; }


    /// <summary>
    /// Get the State of the particular type
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <returns>The specific IState</returns>
    public TState GetState<TState>()
    {
      Type stateType = typeof(TState);
      return (TState)GetState(stateType);
    }

    private object GetState(Type aType)
    {
      using (Logger.BeginScope(nameof(GetState)))
      {
        string typeName = aType.FullName;
        Logger.LogDebug($"{GetType().Name}: {nameof(this.GetState)} typeName:{typeName}");

        if (!States.TryGetValue(typeName, out IState state))
        {
          Logger.LogDebug($"{GetType().Name}: Creating State of type: {typeName}");
          state = (IState)Activator.CreateInstance(aType);
          States.Add(typeName, state);
        }
        else
          Logger.LogDebug($"{GetType().Name}: State exists: {state.Guid}");
        return state;
      }
    }

    /// <summary>
    /// Set the state for specific Type
    /// </summary>
    /// <param name="aNewState"></param>
    public void SetState(IState aNewState)
    {
      string typeName = aNewState.GetType().FullName;
      SetState(typeName, aNewState);
    }

    private void SetState(string aTypeName, object aNewState)
    {
      var newState = (IState)aNewState;
      Logger.LogDebug($"{GetType().Name}: {nameof(SetState)}: typeName:{aTypeName}: Guid:{newState.Guid}");
      States[aTypeName] = newState;
    }
  }
}