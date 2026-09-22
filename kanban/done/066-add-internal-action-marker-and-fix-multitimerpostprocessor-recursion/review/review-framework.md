# Review framework — task 066

**Date:** 2026-09-22
**Host task:** kanban/in-progress/066-add-internal-action-marker-and-fix-multitimerpostprocessor-recursion/
**Diff scope:** branch `task/066-add-internal-action-marker-and-fix-multitimerpostp` vs `origin/master` (product commit `faa6ae74`; kitchen `2f8fff90`)
**Plan / brief:** Code-review 2026-06-11 findings 6 and 29. Guard `MultiTimerPostProcessor` so `ResetTimersOnActivity` does not recurse through the same pipeline. Introduce `IInternalAction` on `ResetTimersOnActivityActionSet.Action`, `StartProcessingActionSet.Action`, and `CompleteProcessingActionSet.Action`. Honor the marker in `MultiTimerPostProcessor` and `ActiveActionBehavior` (replace `EnsureNotType`). Audit other open behaviors/processors. Update timers feature readme registration.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle 01a0c63c-b90a-7d70-915c-609c54b6704f (2026-09-22)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/state/i-internal-action.cs` (new)
- `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.reset-timers-on-activity.cs`
- `source/timewarp-state-plus/features/timers/readme.md`
- `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`
- `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.start-processing.cs`
- `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.complete-processing.cs`
- `tests/timewarp-state-plus-tests/features/timers/multi-timer-post-processor-tests.cs` (new)
- `tests/timewarp-state-plus-tests/features/action-tracking/active-action-behavior-tests.cs` (new)

Surrounding call sites (not modified, still in review scope for regressions):

- Other pipeline behaviors/post-processors: `RenderSubscriptionsPostProcessor`, `StateTransactionBehavior`, `StateInitializationPreProcessor`, `PersistentStatePostProcessor`, `ReduxDevToolsBehavior`
- Sample `EventStreamBehavior` (test-app; still type-skips `AddEventActionSet.Action`)
- `IAction` and action-set source generator (`ResetTimersOnActivity` still sends through `Sender.Send`)

## Requirements to check

- Recursion: `MultiTimerPostProcessor` does not re-reset on `IInternalAction` (including `ResetTimersOnActivityActionSet.Action`); nested send terminates
- Marker: `IInternalAction` extends `IAction`; the three bookkeeping actions implement it
- `ActiveActionBehavior` skips `IInternalAction` instead of `EnsureNotType` throws; `ArgumentValidation` is gone
- `ActiveActionBehavior` still tracks `[TrackAction]` user actions (Start then Complete)
- Audit of who skips vs who still runs is coherent (task notes)
- Timers readme registers via `[assembly: MediatorBehavior(..., Scope = typeof(ClientPipeline))]`, not `IRequestPostProcessor<,>`
- Tests prove: user request sends reset once; nested re-entry of the reset closed generic terminates; start/complete send nothing; tracking skip vs track
- Task 067 render suppression is out of scope

## Round 2

Re-review after M1 fix: `MultiTimerPostProcessor` injects `ISender<ClientPipeline>` and sends `ResetTimersOnActivityActionSet.Action` after `next()`. Carry M1 as fixed or reopen. Scan the fix delta for new defects.
