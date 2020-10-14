namespace BlazorState
{
  using BlazorState.Features.Routing;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Text.Json;

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
      var states = new Dictionary<string, object>();
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
      Dictionary<string, object> newStates = JsonSerializer.Deserialize<Dictionary<string, object>>(aJsonString, JsonSerializerOptions);
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

      Dictionary<string, object> newStateKeyValuePairs =
        JsonSerializer.Deserialize<Dictionary<string, object>>(aKeyValuePair.Value.ToString(), JsonSerializerOptions);
      // Get the Type
      Type stateType = AppDomain.CurrentDomain.GetAssemblies()
          .Where(aAssembly => !aAssembly.IsDynamic)
          .SelectMany(aAssembly => aAssembly.GetTypes())
          .FirstOrDefault(aType => aType.FullName.Equals(typeName));

      Logger.LogDebug($"stateType == null{stateType == null}");

      // Get the Hydrate Method
      // I am only trying to get the name of "Hydrate" without magic string.
      // I use RouteState as a type because it is in this project
      System.Reflection.MethodInfo hydrateMethodInfo = stateType?.GetMethod(nameof(State<RouteState>.Hydrate));
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