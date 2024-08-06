namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  public delegate bool ParameterTrigger(object? previous, object? current);
  public delegate object ParameterGetter();
  public delegate bool TypedTrigger<in T>(T previous, T current);
  public delegate T ParameterSelector<out T>();

  private readonly ConcurrentDictionary<string, (ParameterGetter Getter, ParameterTrigger Trigger)> ParameterTriggers = new();
  private bool ParameterChanged;

  protected void RegisterParameterTrigger<T>(Expression<ParameterSelector<T>> parameterSelector, TypedTrigger<T>? customTrigger = null)
    where T : class
  {
    ArgumentNullException.ThrowIfNull(parameterSelector);

    var memberExpression = (MemberExpression)parameterSelector.Body;
    string parameterName = memberExpression.Member.Name;

    ParameterSelector<T> compiledSelector = parameterSelector.Compile();
    ParameterGetter objectGetter = () => compiledSelector()!;

    ParameterTriggers[parameterName] = (objectGetter, CreateParameterTriggerFunc(customTrigger));
  }

  private static ParameterTrigger CreateParameterTriggerFunc<T>(TypedTrigger<T>? customTrigger = null)
  {
    return (previous, current) =>
    {
      if (previous == null && current == null) return false;
      if (previous == null || current == null) return true;

      T previousValue = (T)previous;
      T currentValue = (T)current;

      if (customTrigger != null)
      {
        return !customTrigger(previousValue, currentValue);
      }

      return !EqualityComparer<T>.Default.Equals(previousValue, currentValue);
    };
  }

  public override Task SetParametersAsync(ParameterView parameters)
  {
    RenderReason = RenderReasonCategory.None;
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
          RenderReason = RenderReasonCategory.ParameterChanged;
          RenderReasonDetail = parameter.Name;
          break;
        }
      }
      else if (ParameterTriggers.TryGetValue(parameter.Name, out (ParameterGetter Getter, ParameterTrigger Trigger) trigger))
      {
        // For registered non-primitive types
        object currentValue = trigger.Getter();
        if (trigger.Trigger(currentValue, parameter.Value))
        {
          ParameterChanged = true;
          RenderReason = RenderReasonCategory.ParameterChanged;
          RenderReasonDetail = parameter.Name;
          break;
        }
      }
      else
      {
        // For unregistered, non-primitive types
        ParameterChanged = true;
        RenderReason = RenderReasonCategory.UntrackedParameter;
        RenderReasonDetail = parameter.Name;
        break;
      }
    }

    if (ParameterChanged) NeedsRerender = true;
    return base.SetParametersAsync(parameters);
  }
}
