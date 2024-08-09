namespace TimeWarp.State;

public abstract partial class TimeWarpStateComponent
{
  private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> TypeParameterProperties = new();
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
  
  /// <summary>
  /// Checks if a parameter has changed.
  /// </summary>
  /// <param name="parameter">The parameter to check.</param>
  /// <returns>True if the parameter has changed, false otherwise.</returns>
  protected bool CheckParameterChanged(ParameterValue parameter)
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

    object? currentValue = property.GetValue(this);
    object? newValue = parameter.Value;
    
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
      changed = CheckComplexParameterChanged(currentValue, newValue);
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
  /// <param name="currentValue">The current value of the parameter.</param>
  /// <param name="newValue">The new value of the parameter.</param>
  /// <returns>
  /// True if the parameter has changed, false otherwise.
  /// </returns>
  /// <remarks>
  /// This method performs a basic reference comparison by default.
  /// Override this method in derived classes to implement custom comparison logic for complex types.
  /// Note: When overriding, be mindful of the performance implications of your custom comparison logic,
  /// especially for large or deeply nested objects.
  /// </remarks>
  protected virtual bool CheckComplexParameterChanged(object? currentValue, object? newValue)
  {
    Logger.LogDebug
    (
      EventIds.TimeWarpStateComponent_CheckComplexParameter
      ,"{ComponentId}: Checking complex parameter: {ValueTypes}"
      ,Id
      ,new
      {
        CurrentType = currentValue?.GetType().Name ?? "null",
        NewType = newValue?.GetType().Name ?? "null"
      }
    );

    bool changed = !ReferenceEquals(currentValue, newValue);

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
          NewValue = newValue
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
