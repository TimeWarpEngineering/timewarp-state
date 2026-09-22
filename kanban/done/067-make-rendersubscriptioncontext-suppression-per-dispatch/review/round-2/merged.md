# Round 2 — merged findings
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
- File: source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs:51
- Description: Round 1: `CompleteDispatch` ran only in a `finally` around the re-render `try`, after `await next()`. A handler throw pinned the instance in the scoped dictionary. The fix wraps the entire `Handle` body in `try`/`finally` (`await next()` at line 53, `GetEnclosingStateType()` at 56, re-render inner `try`/`catch` at 58–83, `CompleteDispatch` at 89). `RenderSubscriptionContext.CompleteDispatch` is unchanged (`TryRemove` on the reference-equality key).
- Suggestion: Wrap the whole `Handle` body so `CompleteDispatch(request)` always runs, including when `next()` throws. Add tests for throw and success paths.
- Source: general (round 1)
- Disposition notes: Round 2 verified outer `try`/`finally` around the whole `Handle`. Inner catch still logs re-render failures and does not swallow `next()` exceptions. Tests `Clear_Instance_Flag_When_Next_Throws` and `Clear_Instance_Flag_After_Successful_Handle` both see `ShouldFireSubscriptionsForAction` true after `Handle`. `dotnet fixie timewarp-state-tests --tests '*RenderSubscriptionsPostProcessor*'` — 8 passed.

## Duplicates / conflicts

None. No new findings in round 2.
