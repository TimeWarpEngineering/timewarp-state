namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  protected void RegisterRenderTrigger<T>(Expression<Func<T, object?>> propertySelector) where T : class
  {
    ArgumentNullException.ThrowIfNull(propertySelector);
    
    Func<T, T, bool> comparisonFunc = CreateComparisonFunc(propertySelector);
    
    RenderTriggers[typeof(T)] = () =>
    {
      T? previousState = GetPreviousState<T>();
      T currentState = GetState<T>(false);
      return previousState == null || comparisonFunc(previousState, currentState);
    };
  }

  private Func<T, T, bool> CreateComparisonFunc<T>(Expression<Func<T, object?>> propertySelector) where T : class
  {
    Func<T, object?> compiledSelector = propertySelector.Compile();
    
    return (T previous, T current) =>
    {
      object? previousValue = compiledSelector(previous);
      object? currentValue = compiledSelector(current);
        
      return !Equals(previousValue, currentValue);
    };
  }

  private string GetPropertyPath<T>(Expression<Func<T, object?>> propertySelector)
  {
    if (propertySelector.Body is MemberExpression memberExpression)
    {
      return GetMemberExpressionPath(memberExpression);
    }
    else if (propertySelector.Body is UnaryExpression unaryExpression)
    {
      if (unaryExpression.Operand is MemberExpression memberExpr)
      {
        return GetMemberExpressionPath(memberExpr);
      }
    }
    throw new ArgumentException("Invalid property selector", nameof(propertySelector));
  }

// ... other methods remain the same
  private string GetMemberExpressionPath(MemberExpression? expression)
  {
    var path = new List<string>();
    while (expression != null)
    {
      path.Add(expression.Member.Name);
      expression = expression.Expression as MemberExpression;
    }
    path.Reverse();
    return string.Join(".", path);
  }

  private object? GetPropertyValue(object? obj, string propertyPath)// Add '?' to 'object?'
  {
    if (obj == null) return null;// Add null check at the beginning

    string[] properties = propertyPath.Split('.');
    object? value = obj;
    foreach (string property in properties)
    {
      if (value == null) return null;
      PropertyInfo? propertyInfo = value.GetType().GetProperty(property);
      if (propertyInfo == null) return null;
      value = propertyInfo.GetValue(value);
    }
    return value;
  }
}
