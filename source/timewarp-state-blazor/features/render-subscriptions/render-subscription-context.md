# Render subscriptions

Re-render after an action is a property of the action type or of the in-flight instance. It is not sticky per-scope state keyed by type name.

## Type-level opt-out

Apply `[SuppressRender]` to an action type that must never re-render subscribers:

```csharp
public sealed class SilentState : State<SilentState>
{
  [SuppressRender]
  public sealed class HeartbeatAction : IAction;
}
```

`RenderSubscriptionsPostProcessor` caches `typeof(TRequest).IsDefined(typeof(SuppressRenderAttribute), inherit: true)` on each closed generic.

`IInternalAction` is not a render skip. Start/Complete processing still re-render ActionTracking UI. A user action that skips render implements `[SuppressRender]` and does not become internal (that would also skip tracking and timer reset).

## Per-dispatch opt-out

`RenderSubscriptionContext.EnsureAction` is obsolete. It keys by action **instance** (reference equality) and `CompleteDispatch` clears the flag when the pipeline completes. A later dispatch of the same type with a new instance re-renders unless that type has `[SuppressRender]`.

`Reset` is test-only. `RemoveAction(string)` is a no-op: type-name keys leaked across dispatches.
