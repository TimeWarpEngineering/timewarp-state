namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Dynamic;
  using System.Linq;
  using BlazorState.Features.Routing;
  using Microsoft.Extensions.Logging;
  using Microsoft.JSInterop;

  /// <summary>
  /// The portion of the store that is only needed to support
  /// ReduxDevTools Integration
  /// </summary>
  internal partial class Store : IReduxDevToolsStore
  {
    /// <summary>
    /// Returns the States in a manner that can be serialized
    /// </summary>
    /// <returns></returns>
    /// <remarks>Used only for ReduxDevTools</remarks>
    public IDictionary<string, object> GetSerializableState()
    {
      var states = (IDictionary<string, object>)new ExpandoObject();
      foreach (KeyValuePair<string, IState> pair in States.OrderBy(aKeyValuePair => aKeyValuePair.Key))
      {
        states[pair.Key] = pair.Value;
      }

      return states;
    }

    /// <summary>
    /// Needed for ReduxDevTools time travel
    /// </summary>
    /// <param name="aJsonString"></param>
    public void LoadStatesFromJson(string aJsonString)
    {
      Logger.LogDebug($"{GetType().Name}:{nameof(LoadStatesFromJson)}: {nameof(aJsonString)}:{aJsonString}");

      Dictionary<string, object> newStates = Json.Deserialize<Dictionary<string, object>>(aJsonString);
      foreach (KeyValuePair<string, object> keyValuePair in newStates)
      {
        LoadStateFromJson(keyValuePair);
      }
    }

    private void LoadStateFromJson(KeyValuePair<string, object> aKeyValuePair)
    {
      string typeName = aKeyValuePair.Key;
      Logger.LogDebug($"{GetType().Name}:{nameof(LoadStatesFromJson)}:typeName: {typeName}");
      // Get the Type
      Type stateType = AppDomain.CurrentDomain.GetAssemblies()
          .Where(aAssembly => !aAssembly.IsDynamic)
          .SelectMany(aAssembly => aAssembly.GetTypes())
          .FirstOrDefault(aType => aType.FullName.Equals(typeName));

      // Get the Hydrate Method
      // I am only trying to get the name of Hydrate
      // I use RouteState as a type because it is in this project
      System.Reflection.MethodInfo hydrateMethodInfo = stateType.GetMethod(nameof(State<RouteState>.Hydrate));

      // Call Hydrate on the Type
      object[] parameters = new object[] { aKeyValuePair.Value.ToString() };
      object currentState = GetState(stateType);
      var newState = (IState)hydrateMethodInfo.Invoke(currentState, parameters);

      // reassign
      SetState(typeName, newState);
    }
  }
}