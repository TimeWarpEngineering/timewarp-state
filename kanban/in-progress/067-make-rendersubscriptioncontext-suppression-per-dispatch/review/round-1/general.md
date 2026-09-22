# Round 1 — general
**Date:** 2026-09-22
**Scope reviewed:** branch `task/067-make-rendersubscriptioncontext-suppression-per-dis` vs `origin/master`

## Summary

The change replaces sticky `FullName` keys with `[SuppressRender]` cached per closed generic (`typeof(TRequest).IsDefined(..., inherit: true)`) and instance-keyed `EnsureAction` (reference equality). `IInternalAction` is not a render skip; Start/Complete still re-render. Obsolete `EnsureAction` / `RemoveAction(string)` / `Reset` match the brief, and `RemoveAction` is a documented no-op. Targeted tests passed (6 post-processor, 6 context). Dominant leftover: `CompleteDispatch` sits in a `finally` that does not wrap `next()`, so a failed dispatch that registered an instance keeps that `IAction` in the scoped dictionary for the WASM app / Server circuit lifetime.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state/features/render-subscriptions/render-subscriptions-post-processor.cs:50
- Description: `CompleteDispatch` runs only in the `finally` around the re-render `try`, after `await next(cancellationToken)`. If the handler (or any inner behavior) throws, the post-processor never reaches that `finally`. `EnsureAction` is the path that inserts into `ConcurrentDictionary<IAction, bool>` with a strong reference-equality key; a failed dispatch therefore pins that action instance for the scoped lifetime and leaves `ShouldFireSubscriptionsForAction` false for a later send of the same object. Re-render of a *new* instance of the same type is still correct (the original FullName leak is fixed), but the stated contract — flags cannot stick for the scoped lifetime; `CompleteDispatch` clears when the pipeline completes — is false on the exception path. `GetEnclosingStateType()` is also outside that `try`/`finally`, so a throw there after a successful `next()` skips cleanup too. Tests never fail `next()` and never re-query the same instance after `Handle`, so they would still pass if `CompleteDispatch` were a no-op. `ActiveActionBehavior` already wraps `next()` in `try`/`finally` for the same class of cleanup.
- Suggestion: Wrap the whole `Handle` body so `CompleteDispatch(request)` always runs, including when `next()` throws (keep the inner catch that logs re-render failures). Add a post-processor test: `EnsureAction(instance, false)`, `next()` throws, then `ShouldFireSubscriptionsForAction(instance)` is true. Optionally assert the same after a successful `Handle` of that instance.
- Status: open
