#region Purpose
// The minimum contract a component needs for TimeWarp.State subscriptions to re-render it (Id, ReRender, ShouldReRender).
#endregion

#region Design
// An interface so any Blazor component can participate without inheriting TimeWarpStateComponent. ShouldReRender
// defaults to true through a default interface method.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Minimum implementation needed for TimeWarp.State to function
/// </summary>
/// <example>
/// <![CDATA[
/// public class YourBaseComponent : BlazorComponent, ITimeWarpStateComponent
/// {
///   static readonly ConcurrentDictionary<string, int> s_InstanceCounts = new();
///   public TimeWarpStateComponent()
///   {
///     string name = GetType().Name;
///     int count = s_InstanceCounts.AddOrUpdate(name, 1, (key, value) => value + 1);
///     Id = $"{name}-{count}";
///   }
///   public string Id { get; }
///   public void ReRender() => StateHasChanged();
/// }
/// ]]>
/// </example>

public interface ITimeWarpStateComponent
{
  string Id { get; }
  
  void ReRender();

  /// <summary>
  /// Determines whether the component should re-render for the given state.
  /// </summary>
  /// <param name="stateType">The type of the state being checked.</param>
  /// <returns>
  /// <c>true</c> if the component should re-render based on the state and its previous values; otherwise, <c>false</c>.
  /// </returns>
  bool ShouldReRender(Type stateType) => true;
}
