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

- [x] Marker: `[SuppressRender]` for render skip. `IInternalAction` stays 066 identity (Start/Complete still re-render). User actions opt out without becoming internal.
- [x] Implement check in RenderSubscriptionsPostProcessor with per-closed-generic caching
- [x] Migrate/obsolete the existing RenderSubscriptionContext API
- [x] Tests: suppressed action type skips re-render; no cross-dispatch leakage between action types or over time
- [x] Implementation review disposition (same task id)

## Notes

066 is merged. `IInternalAction` is pipeline-bookkeeping identity, not a render skip. `[SuppressRender]` is the type-level render opt-out because Start/Complete processing must still re-render ActionTracking UI, and a user action that skips render must not also skip tracking or timer reset. Suppression is per closed generic (`[SuppressRender]`) or per in-flight instance (`EnsureAction`). `Reset` stays test-only.

## Session

- Created: code review 2026-06-11
- 2026-09-22: cockpit — 066 on master; dispatch after 604 merge
- Implementer: grok session 01a0c6d1-d479-7320-9058-ce3e7b3e42a4 (2026-09-22)
- Review oracle: grok session 01a0c6fa-c9bb-7e91-9c23-0ec6cbffbe7c (2026-09-22) — tw-implementation-review effort 1, roster general (round 1 `01a0c6fc`, M1 fix, round 2 `01a0c703`); disposition clean

## Results

Render skip is `[SuppressRender]` on the action type, cached as `typeof(TRequest).IsDefined(typeof(SuppressRenderAttribute), inherit: true)` on each `RenderSubscriptionsPostProcessor<,>` closed generic. `IInternalAction` still re-renders so ActionTracking Start/Complete update the UI. `RenderSubscriptionContext` keys flags by action **instance** (reference equality); `CompleteDispatch` runs in `finally` around the whole `Handle`, including when `next()` throws. `EnsureAction` / `RemoveAction(string)` / `Reset` are obsolete. `RemoveAction` is a no-op (type-name keys leaked). `Reset` remains for tests that call the obsolete surface without the pipeline.

**Files changed**

- `source/timewarp-state/attributes/suppress-render-attribute.cs` (new)
- `source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs`
- `source/timewarp-state/features/render-subscriptions/render-subscription-context.cs`
- `source/timewarp-state/features/render-subscriptions/render-subscription-context.md`
- `source/timewarp-state/state/i-internal-action.cs` (Design region)
- `documentation/overview.md`
- `tests/timewarp-state-tests/pipeline/render-subscriptions-post-processor-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`
- `tests/client-integration-tests/pipeline/render-subscription-context-tests.cs`

**Decisions**

- `[SuppressRender]` is a second marker with a different meaning from `IInternalAction`. Reusing `IInternalAction` as the render skip would hide ActionTracking UI and force user opt-out to skip MultiTimer reset and tracking.
- Obsolete the sticky surface instead of deleting it: binary consumers keep compiling; instance keys plus `CompleteDispatch` stop the FullName leak even if someone still calls `EnsureAction`.
- No new non-obsolete per-dispatch API: nothing in the library called `EnsureAction`. Type-level `[SuppressRender]` is the supported opt-out.

**Review**

- Effort 1; roster: general. Rounds: 2.
- Round 1: M1 bug — `CompleteDispatch` did not run when `next()` threw; instance stayed in the scoped dictionary.
- Round 2: re-verified M1; 0 new findings.
- Final: 0 open; 1 bug fixed; 0 wontfix.
- **Disposition: clean** (`review/disposition.md`; framework `review/review-framework.md`; last ledger `review/round-2/merged.md`).

**Tests**

- `dotnet fixie timewarp-state-tests` — 48 passed, 1 skipped (8 post-processor tests, including throw-path cleanup)
- `dotnet fixie client-integration-tests --tests '*RenderSubscriptionContext*'` — 6 passed (includes `Send_Still_ReRenders_After_EnsureAction_On_Another_Instance`)
- `dotnet fixie timewarp-state-plus-tests` — 31 passed, 1 skipped (`ActionTracking_Should` / `ActiveActionBehavior` unchanged; implement session)

### How to validate

**Smoke**

```bash
dotnet fixie timewarp-state-tests --tests '*RenderSubscriptionsPostProcessor*'
dotnet fixie client-integration-tests --tests '*RenderSubscriptionContext*'
```

**Expect**

- Exit 0.
- `Should_.Skip_ReRender_When_Action_Has_SuppressRender` — 0 re-renders.
- `Should_.ReRender_When_Action_Is_Not_Suppressed` and `Should_.ReRender_When_Action_Is_Internal_Without_SuppressRender` — 1 re-render each.
- `Should_.Not_Leak_Suppression_To_Later_Dispatch_Of_Same_Action_Type` — first (EnsureAction false) instance 0, second instance of the same type 1.
- `Should_.Not_Leak_Suppression_To_Other_Action_Types` — other type still re-renders while the registered instance stays suppressed.
- `Should_.Clear_Instance_Flag_When_Next_Throws` — after `EnsureAction` and a throwing `next()`, `ShouldFireSubscriptionsForAction` is true.
- `Should_.Clear_Instance_Flag_After_Successful_Handle` — same instance fires if queried after a successful skip.
- `RenderSubscriptionContext_Should.Send_Still_ReRenders_After_EnsureAction_On_Another_Instance` — pipeline Send of IncrementCount re-renders after EnsureAction on a dummy instance of the same type.

**Automated gate**

```bash
dotnet fixie timewarp-state-tests
# expect: 48 passed, 1 skipped

dotnet fixie client-integration-tests
# expect: 44 passed, 1 skipped
```

**Not in scope:** Playwright; removing `RenderSubscriptionContext` from DI; applying `[SuppressRender]` to existing library actions.
