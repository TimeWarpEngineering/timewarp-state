namespace BlazorState;

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
      string stateKey = BlazorStateOptions.UseFullNameForStatesInDevTools
        ? pair.Key
        : pair.Key.Split('.').Last();
      
      states[stateKey] = pair.Value;
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

    Logger.LogDebug
    (
      EventIds.LoadStatesFromJson,
      "jsonString:{aJsonString}",
      aJsonString
    );

    Dictionary<string, object> newStates = JsonSerializer.Deserialize<Dictionary<string, object>>(aJsonString, JsonSerializerOptions);

    foreach (KeyValuePair<string, object> keyValuePair in newStates)
    {
      LoadStateFromJson(keyValuePair);
    }
  }

  private void LoadStateFromJson(KeyValuePair<string, object> aKeyValuePair)
  {
    string typeName = aKeyValuePair.Key;
    Logger.LogDebug
    (
      EventIds.LoadStateFromJson,
      "typename:{TypeName} aKeyValuePair.Value: {aKeyValuePair_Value} aKeyValuePair.Value.GetType().Name: {aKeyValuePair_Value_Type_Name}",
      typeName,
      aKeyValuePair.Value,
      aKeyValuePair.Value.GetType().Name
    );

    Dictionary<string, object> newStateKeyValuePairs =
      JsonSerializer.Deserialize<Dictionary<string, object>>(aKeyValuePair.Value.ToString(), JsonSerializerOptions);
    // Get the Type
    Type stateType = AppDomain.CurrentDomain.GetAssemblies()
        .Where(aAssembly => !aAssembly.IsDynamic)
        .SelectMany(aAssembly => aAssembly.GetTypes())
        .FirstOrDefault(aType => aType.FullName.Equals(typeName));

    // Get the Hydrate Method
    // I am only trying to get the name of "Hydrate" without magic string.
    // I use RouteState as a type because it is in this project
    MethodInfo hydrateMethodInfo = stateType?.GetMethod(nameof(State<RouteState>.Hydrate));

    if (hydrateMethodInfo == null)
    {
      throw new NotImplementedException($"The Hydrate Method was not found for the type:{typeName}");
    }

    // Call Hydrate on the Type
    object[] parameters = new object[] { newStateKeyValuePairs };
    object currentState = GetState(stateType);

    var newState = (IState)hydrateMethodInfo.Invoke(currentState, parameters);

    // reassign
    SetState(typeName, newState);
  }
}
