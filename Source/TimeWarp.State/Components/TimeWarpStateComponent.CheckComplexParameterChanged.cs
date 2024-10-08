namespace TimeWarp.State;

public abstract partial class TimeWarpStateComponent
{
  private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> TypeParameterProperties = new();
  private static readonly ConcurrentDictionary<Type, bool> HasOverriddenMethods = new();
  private bool ParameterTriggered;
  private bool SetParametersAsyncWasCalled;
  public string? SetParametersAsyncWasCalledBy  { get; private set; }

  private Dictionary<string, PropertyInfo> ParameterProperties => 
    TypeParameterProperties.GetOrAdd(GetType(), type => 
      type.GetProperties()
        .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null || 
          p.GetCustomAttribute<CascadingParameterAttribute>() != null)
        .ToDictionary(p => p.Name)
    );

  public override Task SetParametersAsync(ParameterView parameters)
  {
    if (Constructed)
    {
      // Logger is property injected so not available in constructor.
      Logger.LogDebug(EventIds.TimeWarpStateComponent_Constructed, "{ComponentId}: created", Id);
      Constructed = false;
    }
    
    if (!CheckForOverriddenMethods())
    {
      // If no methods are overridden, we can return the base result immediately
      // In Should render this won't have set the SetParametersAsyncWasCalled and thus will look like the base behaviour.
      return base.SetParametersAsync(parameters);
    }
    SetParametersAsyncWasCalled = true;
    SetParametersAsyncWasCalledBy = new StackTrace().GetFrame(1)?.GetMethod()?.Name ?? "Unknown";
    foreach (ParameterValue parameter in parameters)
    {
      if (CheckParameterChanged(parameter))
      {
        ParameterTriggered = true;
        break;
      }
    }
    return base.SetParametersAsync(parameters);
  }
  
  private bool CheckForOverriddenMethods()
  {
    return HasOverriddenMethods.GetOrAdd(GetType(), type =>
    {
      var baseType = typeof(TimeWarpStateComponent);
      var virtualMethods = new[] 
      { 
        nameof(CheckPrimitiveParameterChanged),
        nameof(CheckCollectionParameterChanged),
        nameof(CheckComplexParameterChanged),
        nameof(HandleUnregisteredParameter)
      };

      foreach (var methodName in virtualMethods)
      {
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (method != null && method.DeclaringType != baseType)
        {
          return true; // Found an overridden method
        }
      }
      return false; // No overridden methods found
    });
  }
  
  /// <summary>
  /// Checks if a parameter has changed.
  /// </summary>
  /// <param name="parameter">The parameter to check.</param>
  /// <returns>True if the parameter has changed, false otherwise.</returns>
  private bool CheckParameterChanged(ParameterValue parameter)
  {
    if (!ParameterProperties.TryGetValue(parameter.Name, out PropertyInfo? property))
    {
      Logger.LogDebug
      (
        EventIds.TimeWarpStateComponent_ParameterChanged
        ,"{ComponentId}: Unregistered parameter detected: {ParameterName}"
        ,Id
        ,parameter.Name
      );
      
      return HandleUnregisteredParameter(parameter);
    }

    object? newValue = property.GetValue(this);
    object? currentValue = parameter.Value;
    
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (currentValue == null && newValue == null)
    {
      return false; // No change if both are null
    }

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (currentValue == null || newValue == null)
    {
      SetRenderReasonForParameterChange(parameter.Name, "Null value change");
      return true; // Consider it changed if one is null and the other isn't
    }
    
    bool changed;
    
    if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
    {
      changed = CheckPrimitiveParameterChanged(currentValue, newValue);
    }
    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
    {
      changed = CheckCollectionParameterChanged(currentValue as IEnumerable, newValue as IEnumerable);
    }
    else
    {
      changed = CheckComplexParameterChanged(parameter.Name, currentValue, newValue);
    }
    
    if (changed)
    {
      SetRenderReasonForParameterChange(parameter.Name);
      Logger.LogDebug
      (
        EventIds.TimeWarpStateComponent_ParameterChanged
        ,"{ComponentId}: Parameter changed: {ParameterName}"
        ,Id
        ,parameter.Name
      );
    }

    return changed;
  }

  private void SetRenderReasonForParameterChange(string parameterName, string detail = "")
  { 
    RenderReason = RenderReasonCategory.ParameterChanged;
    RenderReasonDetail = string.IsNullOrEmpty(detail) 
      ? $"Parameter '{parameterName}' changed" 
      : $"Parameter '{parameterName}' changed: {detail}";

    Logger.LogDebug
    (
      EventIds.TimeWarpStateComponent_ParameterChanged
      ,"{ComponentId}: Parameter changed: {ParameterDetails}"
      ,Id
      ,new
      {
        Name = parameterName
        ,Detail = detail  
      }
    );
  }
  
  protected virtual bool CheckPrimitiveParameterChanged(object? currentValue, object? newValue)
  {
    return !Equals(currentValue, newValue);
  }

  protected virtual bool CheckCollectionParameterChanged(IEnumerable? currentValue, IEnumerable? newValue)
  {
    // Implement collection comparison logic
    // This is a simplistic check, you might want to implement a more thorough comparison
    return currentValue?.Cast<object>().Count() != newValue?.Cast<object>().Count();
  }

  /// <summary>
  /// Checks if a complex parameter has changed.
  /// </summary>
  /// <param name="parameterName"></param>
  /// <param name="currentValue">The current value of the parameter.</param>
  /// <param name="incomingValue">The new value of the parameter.</param>
  /// <returns>
  /// True if the parameter has changed, false otherwise.
  /// </returns>
  /// <remarks>
  /// This method performs a basic reference comparison by default.
  /// Override this method in derived classes to implement custom comparison logic for complex types.
  /// Note: When overriding, be mindful of the performance implications of your custom comparison logic,
  /// especially for large or deeply nested objects.
  /// </remarks>
  protected virtual bool CheckComplexParameterChanged(string parameterName, object currentValue, object incomingValue)
  {
    Logger.LogDebug
    (
      EventIds.TimeWarpStateComponent_CheckComplexParameter
      ,"{ComponentId}: Checking complex parameter: {ValueTypes}"
      ,Id
      ,new
      {
        ParameterName = parameterName,
        CurrentType = currentValue?.GetType().Name ?? "null",
        IncomingType = incomingValue?.GetType().Name ?? "null"
      }
    );

    bool changed = !ReferenceEquals(currentValue, incomingValue);

    if (changed)
    {
      Logger.LogDebug
      (
        EventIds.TimeWarpStateComponent_ComplexParameterChanged
        ,"{ComponentId}: Complex parameter changed: {Values}"
        ,Id
        ,new
        {
          CurrentValue = currentValue,
          IncomingValue = incomingValue
        }
      );
    }

    return changed;
  }

  protected virtual bool HandleUnregisteredParameter(ParameterValue parameter)
  {
    // Default implementation for unregistered parameters
    return false;
  }
}
