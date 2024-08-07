namespace TimeWarp.State;

public abstract partial class TimeWarpStateComponent
{
  private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> TypeParameterProperties = new();

  private Dictionary<string, PropertyInfo> ParameterProperties => 
    TypeParameterProperties.GetOrAdd(GetType(), type => 
      type.GetProperties()
        .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null || 
          p.GetCustomAttribute<CascadingParameterAttribute>() != null)
        .ToDictionary(p => p.Name)
    );

  protected override void OnInitialized()
  {
    Logger.LogDebug(EventIds.TimeWarpStateComponent_Constructed, "{Id}: created", Id);
    base.OnInitialized();
    // The ParameterProperties will be lazily initialized when first accessed
  }

  public override Task SetParametersAsync(ParameterView parameters)
  {
    foreach (ParameterValue parameter in parameters)
    {
      if (CheckParameterChanged(parameter))
      {
        NeedsRerender = true;
        break;
      }
    }
    return base.SetParametersAsync(parameters);
  }
  
  protected bool CheckParameterChanged(ParameterValue parameter)
  {
    if (!ParameterProperties.TryGetValue(parameter.Name, out PropertyInfo? property))
    {
      return HandleUnregisteredParameter(parameter);
    }

    object? currentValue = property.GetValue(this);
    object newValue = parameter.Value;

    if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
    {
      return CheckPrimitiveParameterChanged(currentValue, newValue);
    }

    if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
    {
      return CheckCollectionParameterChanged(currentValue as IEnumerable, newValue as IEnumerable);
    }

    return CheckComplexParameterChanged(currentValue, newValue);
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

  protected virtual bool CheckComplexParameterChanged(object? currentValue, object? newValue)
  {
    // Implement complex object comparison logic
    // This is a basic reference check, override in derived classes for more specific logic
    return !ReferenceEquals(currentValue, newValue);
  }

  protected virtual bool HandleUnregisteredParameter(ParameterValue parameter)
  {
    // Default implementation for unregistered parameters
    return false;
  }
}
