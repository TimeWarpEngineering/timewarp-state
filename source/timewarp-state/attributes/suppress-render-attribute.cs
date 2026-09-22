#region Purpose
// Type-level opt-out so an action never triggers subscriber re-render.
#endregion

#region Design
// Distinct from IInternalAction: Start/Complete processing must still re-render ActionTracking UI,
// and a user action that skips render must not also skip tracking or timer reset.
// RenderSubscriptionsPostProcessor caches IsDefined per closed generic.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Marks an action type that must not re-render subscribers after it is handled.
/// </summary>
/// <remarks>
/// Checked by <c>RenderSubscriptionsPostProcessor</c> and cached per closed generic.
/// Use this for user actions that opt out of re-render. Pipeline bookkeeping uses
/// <see cref="IInternalAction"/>, which still re-renders (ActionTracking and timer UI).
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SuppressRenderAttribute : Attribute;
