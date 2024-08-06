namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  private delegate bool ParameterTrigger(object? previous, object? current);
  private delegate object ParameterGetter();
  protected delegate bool TypedTrigger<in T>(T previous, T current);
  protected delegate T ParameterSelector<out T>();

  private readonly ConcurrentDictionary<string, (ParameterGetter Getter, ParameterTrigger Trigger)> ParameterTriggers = new();

  protected void RegisterParameterTrigger<T>(Expression<ParameterSelector<T>> parameterSelector, TypedTrigger<T>? customTrigger = null)
    where T : class
  {
    ArgumentNullException.ThrowIfNull(parameterSelector);

    var memberExpression = (MemberExpression)parameterSelector.Body;
    string parameterName = memberExpression.Member.Name;

    ParameterSelector<T> compiledSelector = parameterSelector.Compile();

    ParameterTriggers[parameterName] = (Getter: () => compiledSelector(), CreateParameterTriggerFunc(customTrigger));
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

  private bool CheckParameterChanged(ParameterValue parameter)
  {
    if (parameter.Value.GetType().IsPrimitive)
      return CheckPrimitiveParameterChanged(parameter);

    if (ParameterTriggers.TryGetValue(parameter.Name, out (ParameterGetter Getter, ParameterTrigger Trigger) trigger))
      return CheckRegisteredParameterChanged(parameter, trigger);
    
    return HandleUnregisteredParameter(parameter);
  }

  private bool CheckPrimitiveParameterChanged(ParameterValue parameter)
  {
    PropertyInfo? property = this.GetType().GetProperty(parameter.Name);
    if (property != null && !Equals(property.GetValue(this), parameter.Value))
    {
      RenderReason = RenderReasonCategory.ParameterChanged;
      RenderReasonDetail = parameter.Name;
      return true;
    }
    return false;
  }

  private bool CheckRegisteredParameterChanged(ParameterValue parameter, (ParameterGetter Getter, ParameterTrigger Trigger) trigger)
  {
    object currentValue = trigger.Getter();
    if (trigger.Trigger(currentValue, parameter.Value))
    {
      RenderReason = RenderReasonCategory.ParameterChanged;
      RenderReasonDetail = parameter.Name;
      return true;
    }
    return false;
  }

  private bool HandleUnregisteredParameter(ParameterValue parameter)
  {
    RenderReason = RenderReasonCategory.UntrackedParameter;
    RenderReasonDetail = parameter.Name;
    return true;
  }
}
