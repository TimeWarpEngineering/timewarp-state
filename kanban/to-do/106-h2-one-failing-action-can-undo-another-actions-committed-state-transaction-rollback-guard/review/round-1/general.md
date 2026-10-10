# Round 1 — general
**Date:** 2026-10-11
**Scope reviewed:** same as framework, plus `source/timewarp-state/store/store.cs` (`GetState`, `SetState`, `RemoveState`, `Reset`)

## Summary

The guard in `state-transaction-behavior.cs` compares the live store instance against the clone this action installed. It restores the snapshot only when they match. This fixes both H2 scenarios: A fails after B commits, and B fails while A is in flight. `Store.GetState` returns the stored instance without wrapping it, so `ReferenceEquals` is sound. The behavior is unchanged for a single action, and nothing blocks. The tests interleave with `TaskCompletionSource` and need no timers. They pin the commit, skip, and restore paths, the 405 log entry, and the notification count. Risk is low.

## Issues

### Issue 1 — Severity: suggestion
- File: documentation/topics/cloning.md:12
- Description: The docs say a failure rolls back only its own clone. They do not say that a failing action's in-place writes, made before a concurrent action cloned the state, are copied into that action's clone and survive once the rollback is skipped. In the double-failure case (A fails and its rollback is skipped, then B fails), B restores A's clone with A's partial writes. The old code reached the same end state, so this is not a regression, but the docs suggest a stronger guarantee than the code gives.
- Suggestion: Add one sentence that names this limit. Full isolation needs per-state serialization, which the task deferred.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-state/features/pipeline/state-transaction-behavior.cs:142
- Description: The skip path also runs when the state was removed (`RemoveState` or `Reset`) and lazily recreated during the action. The warning then says "a concurrent action advanced the state", which is not quite true. Skipping is still correct, because restoring would bring back a removed state.
- Suggestion: Optionally reword the message to "the store no longer holds this action's clone".
- Status: open
