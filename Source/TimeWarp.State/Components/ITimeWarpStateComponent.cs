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

  bool ShouldReRender(Type type) => true;
}
