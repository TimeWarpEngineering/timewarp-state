# Round 2 — general
**Date:** 2026-09-22
**Scope reviewed:** fix delta for M1 on branch `task/066-add-internal-action-marker-and-fix-multitimerpostp`

## Summary

The uncommitted M1 fix replaces the transient `TimerState` injection with `ISender<ClientPipeline>` and sends `ResetTimersOnActivityActionSet.Action` after `next()`, matching `ActiveActionBehavior`. Tests construct processors with a recording sender and no longer hand-assign `TimerState.Sender`. Recursion skip is unchanged: internal requests still return after `next()` without a send. `dotnet fixie timewarp-state-plus-tests --tests '*MultiTimerPostProcessor*'` — 3 passed.

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:52
- Notes: Ctor now takes `ISender<ClientPipeline>` (lines 28–36). After `next()` and the `IInternalAction` early return, the processor `await Sender.Send(new ResetTimersOnActivityActionSet.Action(), cancellationToken)` instead of calling `TimerState.ResetTimersOnActivity()` on a DI transient whose `Sender` was never assigned. The handler still uses `Store.GetState<TimerState>()`, so the nested send hits the canonical instance. Tests drop `TimerState` from `ProcessorHarness` and pass the recording sender into every closed generic; `Dispatch_Reset_Once_For_User_Request_And_Skip_On_Internal` and `Terminate_When_Nested_Dispatch_Runs_The_Same_Processor` still prove one send and no re-entry.

## Issues

No new issues found.
