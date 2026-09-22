# Round 1 — merged findings
**Date:** 2026-09-22
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs:50
- Description: `CompleteDispatch` runs only in the `finally` around the re-render `try`, after `await next(cancellationToken)`. If the handler (or any inner behavior) throws, the post-processor never reaches that `finally`. `EnsureAction` is the path that inserts into `ConcurrentDictionary<IAction, bool>` with a strong reference-equality key; a failed dispatch therefore pins that action instance for the scoped lifetime and leaves `ShouldFireSubscriptionsForAction` false for a later send of the same object. Re-render of a *new* instance of the same type is still correct (the original FullName leak is fixed), but the stated contract — flags cannot stick for the scoped lifetime; `CompleteDispatch` clears when the pipeline completes — is false on the exception path. `GetEnclosingStateType()` is also outside that `try`/`finally`, so a throw there after a successful `next()` skips cleanup too. Tests never fail `next()` and never re-query the same instance after `Handle`, so they would still pass if `CompleteDispatch` were a no-op. `ActiveActionBehavior` already wraps `next()` in `try`/`finally` for the same class of cleanup.
- Suggestion: Wrap the whole `Handle` body so `CompleteDispatch(request)` always runs, including when `next()` throws (keep the inner catch that logs re-render failures). Add a post-processor test: `EnsureAction(instance, false)`, `next()` throws, then `ShouldFireSubscriptionsForAction(instance)` is true. Optionally assert the same after a successful `Handle` of that instance.
- Source: general
- Disposition notes: Outer `try`/`finally` around the whole `Handle` so `CompleteDispatch(request)` runs when `next()` throws and when `GetEnclosingStateType()` throws. Inner catch still logs re-render failures. Tests: `Clear_Instance_Flag_When_Next_Throws`, `Clear_Instance_Flag_After_Successful_Handle`. `dotnet fixie timewarp-state-tests --tests '*RenderSubscriptionsPostProcessor*'` — 8 passed.

## Duplicates / conflicts

None — single reviewer, one issue.
