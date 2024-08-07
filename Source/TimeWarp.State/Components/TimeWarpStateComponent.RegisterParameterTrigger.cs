// namespace TimeWarp.State;
//
// using TypeSupport.Extensions;
//
// public partial class TimeWarpStateComponent
// {
//   private delegate bool ParameterTrigger(object? previous, object? current);
//   private delegate object ParameterGetter();
//   protected delegate bool TypedTrigger<in T>(T previous, T current);
//   protected delegate T ParameterSelector<out T>();
//
//   private readonly ConcurrentDictionary<string, (ParameterGetter Getter, ParameterTrigger Trigger)> ParameterTriggers = new();
//
//   protected void RegisterParameterTrigger<T>(Expression<ParameterSelector<T>> parameterSelector, TypedTrigger<T>? customTrigger = null)
//     where T : class
//   {
//     ArgumentNullException.ThrowIfNull(parameterSelector);
//
//     var memberExpression = (MemberExpression)parameterSelector.Body;
//     string parameterName = memberExpression.Member.Name;
//
//     ParameterSelector<T> compiledSelector = parameterSelector.Compile();
//
//     ParameterTriggers[parameterName] = (Getter: () => compiledSelector(), CreateParameterTriggerFunc(customTrigger));
//   }
//
//   private static ParameterTrigger CreateParameterTriggerFunc<T>(TypedTrigger<T>? customTrigger = null)
//   {
//     return (previous, current) =>
//     {
//       if (previous == null && current == null) return false;
//       if (previous == null || current == null) return true;
//
//       T previousValue = (T)previous;
//       T currentValue = (T)current;
//
//       if (customTrigger != null)
//       {
//         return !customTrigger(previousValue, currentValue);
//       }
//
//       return !EqualityComparer<T>.Default.Equals(previousValue, currentValue);
//     };
//   }
//
//   public override Task SetParametersAsync(ParameterView parameters)
//   {
//     RenderReason = RenderReasonCategory.None;
//     RenderReasonDetail = null;
//   
//     foreach (ParameterValue parameter in parameters)
//     {
//       if (CheckParameterChanged(parameter))
//       {
//         NeedsRerender = true;
//         break;
//       }
//     }
//
//     return base.SetParametersAsync(parameters);
//   }
//
//   private bool CheckParameterChanged(ParameterValue parameter)
//   {
//     // Handle null value case
//     if (parameter.Value == null)
//     {
//       Logger.LogWarning("Parameter {Parameter} on Component {ComponentType} has a null value", parameter.Name, GetType().Name);
//       return true; // Assuming null always triggers a change
//     }
//
//     // Check if the parameter is registered first
//     if (ParameterTriggers.TryGetValue(parameter.Name, out (ParameterGetter Getter, ParameterTrigger Trigger) trigger))
//     {
//       return CheckRegisteredParameterChanged(parameter, trigger);
//     }
//
//     // If not registered, then we check for primitive types
//     Type valueType = parameter.Value.GetType();
//
//     if (IsPrimitiveOrSimpleType(valueType))
//     {
//       return CheckPrimitiveParameterChanged(parameter);
//     }
//
//     // If it's not registered and not a primitive type, handle it as an unregistered parameter
//     return HandleUnregisteredParameter(parameter);
//   }
//
//   private bool IsPrimitiveOrSimpleType(Type type)
//   {
//     return type.IsPrimitive 
//       || type == typeof(string) 
//       || type == typeof(decimal)
//       || type == typeof(DateTime)
//       || type == typeof(Guid);
//     // Add any other types you want to treat as "simple"
//   }
//   
//   private Type GetParameterType(string parameterName)
//   {
//     // Check for regular parameters (properties with [Parameter] attribute)
//     var parameterProperty = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
//       .FirstOrDefault(p => p.GetCustomAttributes(typeof(ParameterAttribute), true).Any() 
//         && string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase));
//     
//     if (parameterProperty != null)
//       return parameterProperty.PropertyType;
//
//     // Check for cascading parameters (fields with [CascadingParameter] attribute)
//     var cascadingParameterField = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
//       .FirstOrDefault(f => f.GetCustomAttributes(typeof(CascadingParameterAttribute), true).Any() 
//         && string.Equals(f.Name, parameterName, StringComparison.OrdinalIgnoreCase));
//     
//     if (cascadingParameterField != null)
//       return cascadingParameterField.FieldType;
//
//     // If we still haven't found it, check for a field that ends with the parameter name
//     // This is because Blazor often generates backing fields for cascading parameters with names like '<EditContext>k__BackingField'
//     var backingField = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
//       .FirstOrDefault(f => f.Name.EndsWith(parameterName, StringComparison.OrdinalIgnoreCase) 
//         && f.GetCustomAttributes(typeof(CascadingParameterAttribute), true).Any());
//
//     if (backingField != null)
//       return backingField.FieldType;
//
//     throw new ArgumentException($"Unknown parameter: {parameterName}");
//   }
//
//   private bool CheckPrimitiveParameterChanged(ParameterValue parameter)
//   {
//     PropertyInfo? property = this.GetType().GetProperty(parameter.Name);
//     if (property != null && !Equals(property.GetValue(this), parameter.Value))
//     {
//       RenderReason = RenderReasonCategory.ParameterChanged;
//       RenderReasonDetail = parameter.Name;
//       return true;
//     }
//     return false;
//   }
//
//   private bool CheckRegisteredParameterChanged(ParameterValue parameter, (ParameterGetter Getter, ParameterTrigger Trigger) trigger)
//   {
//     object currentValue = trigger.Getter();
//     if (trigger.Trigger(currentValue, parameter.Value))
//     {
//       RenderReason = RenderReasonCategory.ParameterChanged;
//       RenderReasonDetail = parameter.Name;
//       return true;
//     }
//     return false;
//   }
//
//   private bool HandleUnregisteredParameter(ParameterValue parameter)
//   {
//     RenderReason = RenderReasonCategory.UntrackedParameter;
//     RenderReasonDetail = parameter.Name;
//     return true;
//   }
// }
