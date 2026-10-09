#region Purpose
// Per-dispatch opt-out for subscriber re-render, keyed by the in-flight action instance.
#endregion

#region Design
// Type FullName keys were sticky on a scoped service for the circuit/WASM lifetime.
// Flags use reference equality on the action instance; CompleteDispatch removes them after
// the post-processor runs so a flag cannot leak to a later dispatch of the same type.
// [SuppressRender] is the supported type-level opt-out. Reset remains for tests that call
// the obsolete surface without going through the pipeline.
#endregion

namespace TimeWarp.Features.RenderSubscriptions;

/// <summary>
/// Per-dispatch control over whether subscriptions re-render after an action is handled.
/// </summary>
/// <remarks>
/// Prefer <see cref="SuppressRenderAttribute"/> on the action type. Instance registration
/// applies only to that action object and is cleared when the pipeline completes.
/// </remarks>
public class RenderSubscriptionContext
{
  private readonly ConcurrentDictionary<IAction, bool> ActionSubscriptionFlags = new(ActionReferenceComparer.Instance);

  /// <summary>
  /// Registers the action instance so the post-processor can skip or allow re-render for this dispatch.
  /// </summary>
  /// <param name="action">The in-flight action instance. A different instance of the same type is unaffected.</param>
  /// <param name="shouldFireSubscriptions">False skips re-render for this instance; true allows it.</param>
  [Obsolete("Apply [SuppressRender] to the action type. This registration is keyed by action instance and is cleared when the pipeline completes.")]
  public void EnsureAction(IAction action, bool shouldFireSubscriptions = false)
  {
    ArgumentNullException.ThrowIfNull(action);
    ActionSubscriptionFlags[action] = shouldFireSubscriptions;
  }

  /// <summary>
  /// Returns true if this action instance should fire subscriptions.
  /// Unregistered instances fire by default.
  /// </summary>
  /// <param name="action">The in-flight action instance.</param>
  public bool ShouldFireSubscriptionsForAction(IAction action)
  {
    ArgumentNullException.ThrowIfNull(action);
    return !ActionSubscriptionFlags.TryGetValue(action, out bool shouldFire) || shouldFire;
  }

  /// <summary>
  /// Clears all instance registrations. Tests that call the obsolete surface without the pipeline use this.
  /// </summary>
  [Obsolete("Test-only. [SuppressRender] does not require a reset.")]
  public void Reset() => ActionSubscriptionFlags.Clear();

  /// <summary>
  /// No longer removes by type name. Type-name keys leaked across dispatches.
  /// </summary>
  /// <param name="actionName">Ignored.</param>
  [Obsolete("Apply [SuppressRender] to the action type. Type-name keys are no longer used.")]
  public void RemoveAction(string actionName)
  {
    _ = actionName;
  }

  /// <summary>
  /// Drops the instance flag after the post-processor has consumed it.
  /// </summary>
  internal void CompleteDispatch(IAction action)
  {
    if (action is null)
    {
      return;
    }

    ActionSubscriptionFlags.TryRemove(action, out _);
  }

  private sealed class ActionReferenceComparer : IEqualityComparer<IAction>
  {
    public static readonly ActionReferenceComparer Instance = new();

    public bool Equals(IAction? leftAction, IAction? rightAction) => ReferenceEquals(leftAction, rightAction);

    public int GetHashCode(IAction action) => RuntimeHelpers.GetHashCode(action);
  }
}
