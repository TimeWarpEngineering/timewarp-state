# Round 2 — general
**Date:** 2026-09-22
**Scope reviewed:** M1 fix delta on `RenderSubscriptionsPostProcessor.Handle` and new post-processor tests vs round 1

## Summary

M1 is fixed. `CompleteDispatch(request)` now sits in a `finally` around the whole `Handle`, so a throw from `next()` or `GetEnclosingStateType()` no longer leaves the instance in the scoped dictionary. The inner catch still logs re-render failures and rethrows; `next()` exceptions are not mis-logged as re-render errors. New tests `Clear_Instance_Flag_When_Next_Throws` and `Clear_Instance_Flag_After_Successful_Handle` prove the flag is gone (`ShouldFireSubscriptionsForAction` true) on both paths. `dotnet fixie timewarp-state-tests --tests '*RenderSubscriptionsPostProcessor*'` — 8 passed. No new defects in the fix delta.

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs:51
- Description: Round 1: `CompleteDispatch` ran only in a `finally` around the re-render `try`, after `await next()`. A handler throw pinned the instance in the scoped dictionary. The fix wraps the entire `Handle` body in `try`/`finally` (`await next()` at line 53, `GetEnclosingStateType()` at 56, re-render inner `try`/`catch` at 58–83, `CompleteDispatch` at 89). `RenderSubscriptionContext.CompleteDispatch` is unchanged (`TryRemove` on the reference-equality key).
- Suggestion: Applied as specified. Tests added: `EnsureAction(instance, false)`, `next()` throws, then `ShouldFireSubscriptionsForAction(instance)` is true; same assertion after a successful `Handle` of that instance (re-render still skipped for that dispatch).
- Status: fixed
