# Review framework — task 067

**Date:** 2026-09-22
**Host task:** kanban/in-progress/067-make-rendersubscriptioncontext-suppression-per-dispatch/
**Diff scope:** branch `task/067-make-rendersubscriptioncontext-suppression-per-dis` vs `origin/master` (product commit `3107a4f6`; kitchen `a1382219`)
**Plan / brief:** Code-review 2026-06-11 finding 12. Sticky `FullName` keys on scoped `RenderSubscriptionContext` suppressed every later dispatch of that action type. Make suppression a property of the dispatch: `[SuppressRender]` cached per closed generic on `RenderSubscriptionsPostProcessor`, plus instance-keyed `EnsureAction` cleared when the pipeline completes. `IInternalAction` stays 066 identity (Start/Complete still re-render). Obsolete sticky `EnsureAction` / `RemoveAction` / `Reset`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle 01a0c6fa-c9bb-7e91-9c23-0ec6cbffbe7c (2026-09-22)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/attributes/suppress-render-attribute.cs` (new)
- `source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs`
- `source/timewarp-state/features/render-subscriptions/render-subscription-context.cs`
- `source/timewarp-state/features/render-subscriptions/render-subscription-context.md`
- `source/timewarp-state/state/i-internal-action.cs` (Design region)
- `documentation/overview.md`
- `tests/timewarp-state-tests/pipeline/render-subscriptions-post-processor-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`
- `tests/client-integration-tests/pipeline/render-subscription-context-tests.cs`

Surrounding call sites (not modified, still in review scope for regressions):

- `IInternalAction` bookkeeping actions (Start/Complete processing, ResetTimersOnActivity)
- `ActiveActionBehavior` / `MultiTimerPostProcessor` skip of `IInternalAction` (must not also skip render)
- Existing `EnsureAction` / `RemoveAction` / `Reset` callers (tests only)

## Requirements to check

- `[SuppressRender]` on an action type skips subscriber re-render; cached per closed generic (`typeof(TRequest).IsDefined(..., inherit: true)`)
- `IInternalAction` without `[SuppressRender]` still re-renders (ActionTracking UI)
- User opt-out uses `[SuppressRender]`, not `IInternalAction`
- Instance `EnsureAction` does not leak across action types or later dispatches of the same type
- `CompleteDispatch` clears the instance flag after the post-processor consumes it; flags cannot stick for the scoped lifetime
- Obsolete surface: `EnsureAction`, `RemoveAction(string)`, `Reset`; `RemoveAction` no-op is documented
- Tests prove skip, re-render, internal still re-renders, and no leakage

## Round 2

Re-review after M1 fix: `CompleteDispatch` in `finally` around the whole `Handle` (including `next()` throw). Carry M1 as fixed or reopen. Scan the fix delta for new defects. Tests: `Clear_Instance_Flag_When_Next_Throws`, `Clear_Instance_Flag_After_Successful_Handle`.
