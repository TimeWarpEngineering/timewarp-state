# Round 1 — general
**Date:** 2026-09-22
**Scope reviewed:** branch `task/066-add-internal-action-marker-and-fix-multitimerpostp` vs `origin/master`

## Summary

The change adds `IInternalAction : IAction`, marks the three pipeline bookkeeping actions, and skips that marker in `MultiTimerPostProcessor` (after `next()`) and `ActiveActionBehavior` (instead of `EnsureNotType` throws). Marker application, skip-vs-run audit of the other open behaviors, MediatorBehavior readme registration, and unit tests all match the brief. `dotnet fixie timewarp-state-plus-tests --tests '*MultiTimerPostProcessor*'` and `--tests '*ActiveActionBehavior*'` were 3 passed each; the nested-re-entry test is a real closed-generic recursion proof (`OnSend` re-enters `ResetProcessor`). Dominant leftover risk: the processor still injects a transient `TimerState` whose `Sender` is never assigned, so a host that follows the new readme will `NullReferenceException` after `next()` rather than reset timers.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:48
- Description: `MultiTimerPostProcessor` injects `TimerState` from DI and calls `TimerState.ResetTimersOnActivity()`. States are `TryAddTransient` (`EnsureStates`); only `Store.GetState` assigns `Sender` (`store.cs:167`). The injected instance is a different transient object with `Sender` still `null!`, so the generated wrapper NREs on `Sender.Send` the first time a host actually weaves this behavior. This constructor was pre-existing, but this commit removes the "do not enable" warning and documents `[assembly: MediatorBehavior(typeof(MultiTimerPostProcessor<,>), order: 550, Scope = typeof(ClientPipeline))]` as the way to turn it on. The new tests hide the gap: `ProcessorHarness` constructs `TimerState` by hand and sets `Sender = Sender` before `Handle`. Sibling behaviors (`ActiveActionBehavior`, `PersistentStatePostProcessor`, `StateTransactionBehavior`) inject `ISender<ClientPipeline>` or `IStore`, not a state type.
- Suggestion: Stop injecting `TimerState`. Either inject `IStore` and `await Store.GetState<TimerState>().ResetTimersOnActivity()` (canonical instance, `Sender` set), or inject `ISender<ClientPipeline>` and `await Sender.Send(new TimerState.ResetTimersOnActivityActionSet.Action(), cancellationToken)` like `ActiveActionBehavior`. Cover the DI path so a harness that forgets to assign `Sender` fails.
- Status: open
