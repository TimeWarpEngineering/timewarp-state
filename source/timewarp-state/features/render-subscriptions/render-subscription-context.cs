namespace TimeWarp.Features.RenderSubscriptions;

/// <summary>
/// Provides control over subscription re-rendering.
/// </summary>
/// <remarks>
/// Handler developers should inject this context and use the EnsureAction method
/// to prevent automatic re-rendering of subscriptions for specific actions.
/// This allows for fine-grained control over the rendering process and can
/// significantly improve application performance.
/// </remarks>
public class RenderSubscriptionContext
{
  private readonly ConcurrentDictionary<string, bool> ActionSubscriptionFlags = new();

  /// <summary>
  /// Registers the action with the context.
  /// Ensures that subscriptions will not be fired for the specified action.
  /// </summary>
  /// <param name="action"></param>
  /// <param name="shouldFireSubscriptions"></param>
  public void EnsureAction(IAction action, bool shouldFireSubscriptions = false)
  {
    ArgumentNullException.ThrowIfNull(action);
    string key = BuildKey(action);
    ActionSubscriptionFlags[key] = shouldFireSubscriptions;
  }

  /// <summary>
  /// Returns true if the action should fire subscriptions.
  /// If the action has not been registered with EnsureAction, then it will fire subscriptions by default.
  /// </summary>
  /// <param name="action"></param>
  /// <returns></returns>
  public bool ShouldFireSubscriptionsForAction(IAction action)
  {
    string key = BuildKey(action);
    return !ActionSubscriptionFlags.TryGetValue(key, out bool shouldFire) || shouldFire;
  }

  /// <summary>
  /// Resets the context.
  /// </summary>
  public void Reset() => ActionSubscriptionFlags.Clear();
  
  /// <summary>
  /// Removes the action from the context.
  /// </summary>
  /// <param name="actionName"></param>
  public void RemoveAction(string actionName)
  {
    ActionSubscriptionFlags.TryRemove(actionName, out _);
  }
  
  private static string BuildKey(IAction action) => 
    action.GetType().FullName 
    ?? throw new InvalidOperationException("Action type name is null");
}
