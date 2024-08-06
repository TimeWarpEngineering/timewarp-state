namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  private readonly ConcurrentDictionary<string, (Func<object> Getter, Func<object?, object?, bool> Comparer)> ParameterComparisons = new();

  protected void RegisterParameterComparison<T>(Expression<Func<T>> parameterSelector, Func<T, T, bool>? customComparison = null)
  {
    ArgumentNullException.ThrowIfNull(parameterSelector);

    var memberExpression = (MemberExpression)parameterSelector.Body;
    string parameterName = memberExpression.Member.Name;

    Func<T> compiledSelector = parameterSelector.Compile();
    Func<object> objectGetter = () => compiledSelector()!;

    ParameterComparisons[parameterName] = (objectGetter, CreateParameterComparisonFunc(customComparison));
  }

  private static Func<object?, object?, bool> CreateParameterComparisonFunc<T>(Func<T, T, bool>? customComparison = null)
  {
    return (previous, current) =>
    {
      if (previous == null && current == null) return false;
      if (previous == null || current == null) return true;

      T previousValue = (T)previous;
      T currentValue = (T)current;

      if (customComparison != null)
      {
        return !customComparison(previousValue, currentValue);
      }

      return !EqualityComparer<T>.Default.Equals(previousValue, currentValue);
    };
  }

  public override Task SetParametersAsync(ParameterView parameters)
  {
    bool parameterChanged = false;

    foreach (ParameterValue parameter in parameters)
    {
      if (ParameterComparisons.TryGetValue(parameter.Name, out var comparison))
      {
        object? currentValue = comparison.Getter();
        if (comparison.Comparer(currentValue, parameter.Value))
        {
          parameterChanged = true;
          break;
        }
      }
    }

    if (parameterChanged)
    {
      return base.SetParametersAsync(parameters);
    }
    else
    {
      // Parameters haven't changed, skip update
      return Task.CompletedTask;
    }
  }
}
