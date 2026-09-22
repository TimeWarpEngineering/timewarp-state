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
- File: source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:52
- Description: `MultiTimerPostProcessor` injected transient `TimerState` whose `Sender` is never assigned (`Store.GetState` is the only assigner). A host following the new MediatorBehavior registration would NRE on the first reset send.
- Suggestion: Inject `ISender<ClientPipeline>` and send `ResetTimersOnActivityActionSet.Action` after `next()`.
- Source: general (round 1)
- Disposition notes: Round 2 verified ctor takes `ISender<ClientPipeline>`; after `next()` and the `IInternalAction` skip, `await Sender.Send(new ResetTimersOnActivityActionSet.Action(), cancellationToken)`. Tests pass the recording sender; no hand-assigned `TimerState.Sender`. Nested re-entry still terminates with send count 1.

## Duplicates / conflicts

None. No new findings in round 2.
