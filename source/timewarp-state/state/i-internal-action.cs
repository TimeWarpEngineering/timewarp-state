#region Purpose
// Type-level marker for pipeline-originated bookkeeping actions that cross-cutting behaviors skip.
#endregion

#region Design
// Identity, not a behavior flag: the pipeline originated this action for its own bookkeeping.
// Cached as typeof(IInternalAction).IsAssignableFrom(typeof(TRequest)) on each closed generic.
// Render suppression is [SuppressRender] (task 067). Start/Complete processing must still
// re-render ActionTracking UI; user actions may also want to skip render.
// StateTransaction, initialization, persistence, and Redux DevTools still run for internals.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Marks an action the pipeline originates for its own bookkeeping.
/// Cross-cutting behaviors skip these so they do not recurse or treat infrastructure as user work.
/// </summary>
public interface IInternalAction : IAction;
