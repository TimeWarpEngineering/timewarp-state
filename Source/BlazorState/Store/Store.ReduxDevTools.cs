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
    foreach (KeyValuePair<string, IState> pair in States.OrderBy(keyValuePair => keyValuePair.Key))
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
  /// <param name="jsonString"></param>
  public void LoadStatesFromJson(string jsonString)
  {
    if (string.IsNullOrWhiteSpace(jsonString))
      throw new ArgumentException("jsonString was null or white space", nameof(jsonString));

    Logger.LogDebug
    (
      EventIds.LoadStatesFromJson,
      "jsonString:{aJsonString}",
      jsonString
    );

    Dictionary<string, object> newStates =
      JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, JsonSerializerOptions)
      ?? throw new InvalidOperationException();

    foreach (KeyValuePair<string, object> keyValuePair in newStates)
    {
      LoadStateFromJson(keyValuePair);
    }
  }

  private void LoadStateFromJson(KeyValuePair<string, object> keyValuePair)
  {
    string typeName = keyValuePair.Key;
    Logger.LogDebug
    (
      EventIds.LoadStateFromJson,
      "typename:{TypeName} keyValuePair.Value: {keyValuePair_Value} keyValuePair.Value.GetType().Name: {keyValuePair_Value_Type_Name}",
      typeName,
      keyValuePair.Value,
      keyValuePair.Value.GetType().Name
    );

    string stringValue = keyValuePair.Value.ToString() ?? throw new InvalidOperationException();
    
    Dictionary<string, object> newStateKeyValuePairs =
      JsonSerializer.Deserialize<Dictionary<string, object>>(stringValue, JsonSerializerOptions) ?? throw new InvalidOperationException();
    
    // Get the Type
    Type stateType = AppDomain.CurrentDomain.GetAssemblies()
      .Where(assembly => !assembly.IsDynamic)
      .SelectMany(assembly => assembly.GetTypes())
      .FirstOrDefault(type => type.FullName?.Equals(typeName) == true) 
      ?? throw new InvalidOperationException();

    // Get the Hydrate Method
    // I am only trying to get the name of "Hydrate" without magic string.
    // I use IState as a type because it is in this project
    MethodInfo hydrateMethodInfo = 
      stateType.GetMethod(nameof(IState<string>.Hydrate)) ?? throw new InvalidOperationException();

    if (hydrateMethodInfo == null)
    {
      throw new NotImplementedException($"The Hydrate Method was not found for the type:{typeName}");
    }

    // Call Hydrate on the Type
    object[] parameters = new object[]
    {
      newStateKeyValuePairs
    };
    object currentState = GetState(stateType);

    var newState = (IState) (hydrateMethodInfo.Invoke(currentState, parameters) ?? throw new InvalidOperationException());

    // reassign
    SetState(typeName, newState);
  }
}
