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
      if (string.IsNullOrWhiteSpace(aJsonString))
        throw new ArgumentException("aJsonString was null or white space", nameof(aJsonString));

      Logger.LogDebug($"{GetType().Name}:{nameof(LoadStatesFromJson)}: {nameof(aJsonString)}:{aJsonString}");
      Dictionary<string, object> newStates = Json.Deserialize<Dictionary<string, object>>(aJsonString);
      Logger.LogDebug($"newStates.Count: {newStates.Count}");
      foreach (KeyValuePair<string, object> keyValuePair in newStates)
      {
        Logger.LogDebug($"keyValuePair.Key:{keyValuePair.Key}");
        Logger.LogDebug($"keyValuePair.Value:{keyValuePair.Value}");
        LoadStateFromJson(keyValuePair);
      }
    }

    private void LoadStateFromJson(KeyValuePair<string, object> aKeyValuePair)
    {
      string typeName = aKeyValuePair.Key;
      Logger.LogDebug($"{GetType().Name}:{nameof(LoadStateFromJson)}:typeName: {typeName}");
      Logger.LogDebug($"aKeyValuePair.Value: {aKeyValuePair.Value}");
      Logger.LogDebug($"aKeyValuePair.Value.GetType().Name: {aKeyValuePair.Value.GetType().Name}");
      //var newStateKeyValuePairs = (Dictionary<string, object>) aKeyValuePair.Value;
      //Logger.LogDebug($"newStateKeyValuePairs.Count: {newStateKeyValuePairs.Count}");
      object newStateKeyValuePairs = Json.Deserialize<object>(aKeyValuePair.Value.ToString());
      //Logger.LogDebug($"newStateKeyValuePairs.Count: {newStateKeyValuePairs.Count}");

      // Get the Type
      Type stateType = AppDomain.CurrentDomain.GetAssemblies()
          .Where(aAssembly => !aAssembly.IsDynamic)
          .SelectMany(aAssembly => aAssembly.GetTypes())
          .FirstOrDefault(aType => aType.FullName.Equals(typeName));

      Logger.LogDebug($"stateType == null{stateType == null}");

      // Get the Hydrate Method
      // I am only trying to get the name of "Hydrate" without magic string.
      // I use RouteState as a type because it is in this project
      System.Reflection.MethodInfo hydrateMethodInfo = stateType.GetMethod(nameof(State<RouteState>.Hydrate));
      Logger.LogDebug($"hydrateMethodInfo == null: {hydrateMethodInfo == null}");

      // Call Hydrate on the Type
      object[] parameters = new object[] { newStateKeyValuePairs };
      object currentState = GetState(stateType);
      Logger.LogDebug("call hydrate method");
      var newState = (IState)hydrateMethodInfo.Invoke(currentState, parameters);

      // reassign
      SetState(typeName, newState);
    }
  }
}