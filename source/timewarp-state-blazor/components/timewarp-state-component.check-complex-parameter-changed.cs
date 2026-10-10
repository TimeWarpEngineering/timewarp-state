#region Purpose
// Detects parameter changes before base.SetParametersAsync applies incoming values.
#endregion

#region Design
// property.GetValue(this) is the current (old) value: this runs before
// base.SetParametersAsync. parameter.Value is the incoming value from ParameterView.
// Virtual comparators take (current, incoming) in that order so directional
// overrides and trace logs match the documented contract.
// HandleUnregisteredParameter returning true must set RenderReasonDetail here;
// derived classes cannot, because the setter is private.
// Value types and string compare with Equals. Other non-collection reference types compare by reference.
// Collections compare with SequenceEqual on a materialized snapshot, so order is significant.
// IQueryable is never enumerated; a different instance counts as changed.
#endregion

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
    if (TimeWarpStateOptions is { CaptureRenderCaller: true })
    {
      SetParametersAsyncWasCalledBy = FormatRenderCaller(new StackTrace().GetFrame(1));
    }
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
      
      bool unregisteredParameterChanged = HandleUnregisteredParameter(parameter);
      if (unregisteredParameterChanged)
      {
        SetRenderReasonForParameterChange(parameter.Name, "Unregistered parameter");
      }

      return unregisteredParameterChanged;
    }

    object? currentValue = property.GetValue(this);
    object? incomingValue = parameter.Value;
    
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (currentValue == null && incomingValue == null)
    {
      return false; // No change if both are null
    }

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (currentValue == null || incomingValue == null)
    {
      SetRenderReasonForParameterChange(parameter.Name, "Null value change");
      return true; // Consider it changed if one is null and the other isn't
    }
    
    bool changed;
    
    if (ComparesByValue(property.PropertyType))
    {
      changed = CheckPrimitiveParameterChanged(currentValue, incomingValue);
    }
    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
    {
      changed = CheckCollectionParameterChanged(currentValue as IEnumerable, incomingValue as IEnumerable);
    }
    else
    {
      changed = CheckComplexParameterChanged(parameter.Name, currentValue, incomingValue);
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

  /// <summary>
  /// Reports whether a collection parameter changed.
  /// </summary>
  /// <param name="currentValue">The collection on the component before this parameter set.</param>
  /// <param name="newValue">The collection supplied by the parent.</param>
  /// <returns>
  /// True when the element snapshots differ, or when either value is an <see cref="IQueryable"/>
  /// that is not the same instance.
  /// </returns>
  /// <remarks>
  /// Elements are copied before they are compared, so a live collection cannot change mid-comparison.
  /// An <see cref="IQueryable"/> is not enumerated.
  /// </remarks>
  protected virtual bool CheckCollectionParameterChanged(IEnumerable? currentValue, IEnumerable? newValue)
  {
    if (ReferenceEquals(currentValue, newValue))
    {
      return false;
    }

    if (currentValue is null || newValue is null)
    {
      return true;
    }

    // Enumerating a query executes it. A different instance is a change.
    if (currentValue is IQueryable || newValue is IQueryable)
    {
      return true;
    }

    object?[] currentItems = Snapshot(currentValue);
    object?[] incomingItems = Snapshot(newValue);
    return !currentItems.SequenceEqual(incomingItems);
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
  /// Value types, enums, and string are compared with Equals and do not reach this method.
  /// Collections are compared element-wise and do not reach this method.
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

  private static bool ComparesByValue(Type type)
  {
    Type comparedType = Nullable.GetUnderlyingType(type) ?? type;
    return comparedType.IsValueType || comparedType == typeof(string);
  }

  private static object?[] Snapshot(IEnumerable values)
  {
    return values.Cast<object?>().ToArray();
  }
}
