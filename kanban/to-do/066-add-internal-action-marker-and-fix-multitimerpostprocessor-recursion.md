# Add internal-action marker and fix MultiTimerPostProcessor recursion

## Description

Code review 2026-06-11, findings 6 and 29 (`code-review-2026-06-11.md`).

`source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:19–23` runs for every `TRequest : notnull` and unconditionally awaits `TimerState.ResetTimersOnActivity()`, which the source generator implements as `Sender.Send(new ResetTimersOnActivityActionSet.Action(), ...)` — a full pipeline pass whose post-processors include `MultiTimerPostProcessor` itself. Nothing breaks the cycle: registering it as the feature readme documents (line 48, `AddScoped(typeof(IRequestPostProcessor<,>), typeof(MultiTimerPostProcessor<,>))`) gives **infinite recursion on the first dispatched action**. (No sample/test registers it today, consistent with it never having been exercised.) At minimum, every user action would pay a second full pipeline pass.

The deeper issue (finding 29): pipeline infrastructure has no general way to exclude internally-originated actions. `ActiveActionBehavior` (`action-tracking-behavior.cs:25`) hand-rolls the same exclusion with runtime `EnsureNotType` throws against two hard-coded action types — the failure mode for forgetting one is a runtime exception or infinite recursion, never a compile error.

## Fix

- Immediate: guard `MultiTimerPostProcessor` — `if (request is ResetTimersOnActivityActionSet.Action) return;`
- General: introduce an internal-action marker (e.g. `IInternalAction` on `ResetTimersOnActivityActionSet.Action`, `StartProcessingActionSet.Action`, `CompleteProcessingActionSet.Action`) honored by cross-cutting behaviors/post-processors, replacing `ActiveActionBehavior`'s hard-coded `EnsureNotType` list.

## Checklist

- [ ] Recursion guard in MultiTimerPostProcessor + test (dispatching any action terminates; reset dispatched exactly once)
- [ ] `IInternalAction` (or equivalent) marker + apply to the three internal actions
- [ ] Replace `EnsureNotType` throws in ActiveActionBehavior with the marker check
- [ ] Audit other open behaviors/processors for whether they should skip internal actions
- [ ] Update timers feature readme registration guidance

## Notes

Coordinate the marker design with task 067 (render suppression), which wants a similar declarative, type-level mechanism.
