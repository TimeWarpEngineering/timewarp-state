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
- File: source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:48
- Description: `MultiTimerPostProcessor` injects `TimerState` from DI and calls `TimerState.ResetTimersOnActivity()`. States are `TryAddTransient` (`EnsureStates`); only `Store.GetState` assigns `Sender` (`store.cs:167`). The injected instance is a different transient object with `Sender` still `null!`, so the generated wrapper NREs on `Sender.Send` the first time a host actually weaves this behavior. This constructor was pre-existing, but this commit removes the "do not enable" warning and documents `[assembly: MediatorBehavior(typeof(MultiTimerPostProcessor<,>), order: 550, Scope = typeof(ClientPipeline))]` as the way to turn it on. The new tests hide the gap: `ProcessorHarness` constructs `TimerState` by hand and sets `Sender = Sender` before `Handle`. Sibling behaviors (`ActiveActionBehavior`, `PersistentStatePostProcessor`, `StateTransactionBehavior`) inject `ISender<ClientPipeline>` or `IStore`, not a state type.
- Suggestion: Stop injecting `TimerState`. Either inject `IStore` and `await Store.GetState<TimerState>().ResetTimersOnActivity()` (canonical instance, `Sender` set), or inject `ISender<ClientPipeline>` and `await Sender.Send(new TimerState.ResetTimersOnActivityActionSet.Action(), cancellationToken)` like `ActiveActionBehavior`. Cover the DI path so a harness that forgets to assign `Sender` fails.
- Source: general
- Disposition notes: Injected `ISender<ClientPipeline>` and send `ResetTimersOnActivityActionSet.Action` after `next()` (same pattern as `ActiveActionBehavior`). Tests construct processors with the recording sender; no transient `TimerState`.

## Duplicates / conflicts

None — single reviewer, one issue.
