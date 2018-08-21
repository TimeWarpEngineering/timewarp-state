namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Dynamic;
  using System.Linq;
  using Microsoft.AspNetCore.Blazor;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  /// <summary>
  /// </summary>
  /// <remarks>TODO: A significant amount of this class is to support
  /// Redux dev tools.  Should we not split this up so that portion
  /// is only included if in dev mode.
  /// </remarks>
  internal class Store : IStore
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

    public Guid Guid { get; } = Guid.NewGuid();
    private ILogger Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IDictionary<string, IState> States { get; }

    public IDictionary<string, object> GetSerializableState()
    {
      var states = (IDictionary<string, object>)new ExpandoObject();
      foreach (KeyValuePair<string, IState> pair in States.OrderBy(x => x.Key))
      {
        states[pair.Key] = pair.Value;
      }

      return states;
    }

    public TState GetState<TState>()
    {
      Type stateType = typeof(TState);
      return (TState)GetState(stateType);
    }

    public object GetState(Type aType)
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

    public void LoadStatesFromJson(string aJsonString)
    {
      Logger.LogDebug($"{GetType().Name}:{nameof(LoadStatesFromJson)}: {nameof(aJsonString)}:{aJsonString}");

      Dictionary<string, object> newStates = Json.Deserialize<Dictionary<string, object>>(aJsonString);
      foreach (KeyValuePair<string, object> keyValuePair in newStates)
      {
        LoadStateFromJson(keyValuePair);
      }
    }

    public void SetState(IState aNewState)
    {
      string typeName = aNewState.GetType().FullName;
      SetState(typeName, aNewState);
    }

    public void SetState(string typeName, object aNewState)
    {
      var newState = (IState)aNewState;
      Logger.LogDebug($"{GetType().Name}: {nameof(SetState)}: typeName:{typeName}: Guid:{newState.Guid}");
      States[typeName] = newState;
    }

    private void LoadStateFromJson(KeyValuePair<string, object> keyValuePair)
    {
      string typeName = keyValuePair.Key;
      Logger.LogDebug($"{GetType().Name}:{nameof(LoadStatesFromJson)}:typeName: {typeName}");
      // Get the Type
      Type stateType = AppDomain.CurrentDomain.GetAssemblies()
          .Where(a => !a.IsDynamic)
          .SelectMany(a => a.GetTypes())
          .FirstOrDefault(t => t.FullName.Equals(typeName));

      // Get the Hydrate Method
      // TODO: remove magic string
      System.Reflection.MethodInfo hydrateMethodInfo = stateType.GetMethod("Hydrate");

      // Call Hydrate on the Type
      object[] parameters = new object[] { keyValuePair.Value.ToString() };
      object currentState = GetState(stateType);
      var newState = (IState)hydrateMethodInfo.Invoke(currentState, parameters);

      // reassign
      SetState(typeName, newState);
    }
  }
}