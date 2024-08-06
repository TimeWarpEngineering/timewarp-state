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
    where T : class
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
      if (parameter.Value.GetType().IsPrimitive)
      {
        // For all primitive types, do a direct comparison
        PropertyInfo? property = this.GetType().GetProperty(parameter.Name);
        if (property != null && !Equals(property.GetValue(this), parameter.Value))
        {
          ParameterChanged = true;
          RenderReasonCategory = RenderReasonCategory.ParameterChanged;
          RenderReasonDetail = parameter.Name;
          break;
        }
      }
      else if (ParameterComparisons.TryGetValue(parameter.Name, out (ParameterGetter Getter, ParameterComparer Comparer) comparison))
      {
        // For registered non-primitive types
        object currentValue = comparison.Getter();
        if (comparison.Comparer(currentValue, parameter.Value))
        {
          ParameterChanged = true;
          RenderReasonCategory = RenderReasonCategory.ParameterChanged;
          RenderReasonDetail = parameter.Name;
          break;
        }
      }
      else
      {
        // For unregistered, non-primitive types
        ParameterChanged = true;
        RenderReasonCategory = RenderReasonCategory.UntrackedParameter;
        RenderReasonDetail = parameter.Name;
        break;
      }
    }

    if (ParameterChanged) NeedsRerender = true;
    return base.SetParametersAsync(parameters);
  }
}
