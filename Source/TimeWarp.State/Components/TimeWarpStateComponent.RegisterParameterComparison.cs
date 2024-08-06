namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  public delegate bool ParameterComparer(object? previous, object? current);
  public delegate object ParameterGetter();
  public delegate bool TypedComparer<in T>(T previous, T current);
  public delegate T ParameterSelector<out T>();

  private readonly ConcurrentDictionary<string, (ParameterGetter Getter, ParameterComparer Comparer)> ParameterComparisons = new();
  private bool ParameterChanged;

  protected void RegisterParameterComparison<T>(Expression<ParameterSelector<T>> parameterSelector, TypedComparer<T>? customComparison = null)
  {
    ArgumentNullException.ThrowIfNull(parameterSelector);

    var memberExpression = (MemberExpression)parameterSelector.Body;
    string parameterName = memberExpression.Member.Name;

    ParameterSelector<T> compiledSelector = parameterSelector.Compile();
    ParameterGetter objectGetter = () => compiledSelector()!;

    ParameterComparisons[parameterName] = (objectGetter, CreateParameterComparisonFunc(customComparison));
  }

  private static ParameterComparer CreateParameterComparisonFunc<T>(TypedComparer<T>? customComparison = null)
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
    RenderReasonCategory = RenderReasonCategory.None;
    RenderReasonDetail = null;
    ParameterChanged = false;

    foreach (ParameterValue parameter in parameters)
    {
      if (ParameterComparisons.TryGetValue(parameter.Name, out (ParameterGetter Getter, ParameterComparer Comparer) comparison))
      {
        object currentValue = comparison.Getter();
        if (comparison.Comparer(currentValue, parameter.Value))
        {
          ParameterChanged = true;
          RenderReasonCategory = RenderReasonCategory.Parameter;
          RenderReasonDetail = parameter.Name;
          break;
        }
      }
      else
      {
        // If the parameter is not registered for comparison, we assume it might have changed
        ParameterChanged = true;
        RenderReasonCategory = RenderReasonCategory.Default;
        RenderReasonDetail = parameter.Name;
        break;
      }
    }

    if (ParameterChanged) NeedsRerender = true;
    return base.SetParametersAsync(parameters);
  }
}
