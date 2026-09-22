# Make RenderSubscriptionContext suppression per-dispatch

## Description

Code review 2026-06-11, finding 12 (`code-review-2026-06-11.md`).

`source/timewarp-state/features/render-subscriptions/render-subscription-context.cs`: render suppression flags are keyed by action-type `FullName` (`BuildKey`, lines 55–57) in a `ConcurrentDictionary` on a scoped service (app-lifetime in WASM, circuit-lifetime in Server). Nothing in the framework ever calls `Reset()`/`RemoveAction()` — only tests do. So one `EnsureAction(action)` call suppresses re-render for **all subsequent and concurrent dispatches of that action type** until a handler manually removes it: a call-order/lifetime trap in the public API.

## Fix

Make suppression a property of the dispatch, not shared mutable state:

- Declarative: a `[SuppressRender]` attribute or marker interface on the action type, checked by `RenderSubscriptionsPostProcessor` (cached per closed generic) — covers "this action type never triggers re-render".
- Per-instance opt-out (if needed): carry the flag on the request/context for the single in-flight dispatch only.
- Deprecate/remove the sticky `EnsureAction`/`RemoveAction`/`Reset` surface.

## Checklist

- [x] Marker: use **066** `IInternalAction` (on master). Do not invent a second marker. `[SuppressRender]` only if user-action types must opt out without implementing `IInternalAction`.
- [ ] Implement check in RenderSubscriptionsPostProcessor with per-closed-generic caching
- [ ] Migrate/obsolete the existing RenderSubscriptionContext API
- [ ] Tests: suppressed action type skips re-render; no cross-dispatch leakage between action types or over time

## Notes

066 is merged. `IInternalAction` is the marker. Suppression must be **per dispatch** (or a static type check cached per closed generic). Do not leave sticky `FullName` keys that survive the outer action. `Reset` stays test-only.

## Session

- Created: code review 2026-06-11
- 2026-09-22: cockpit — 066 on master; dispatch after 604 merge
