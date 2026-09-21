# Add internal-action marker and fix MultiTimerPostProcessor recursion

## Description

Code review 2026-06-11, findings 6 and 29 (`code-review-2026-06-11.md`).

`source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:19–23` runs for every `TRequest : notnull` and unconditionally awaits `TimerState.ResetTimersOnActivity()`, which the source generator implements as `Sender.Send(new ResetTimersOnActivityActionSet.Action(), ...)` — a full pipeline pass whose post-processors include `MultiTimerPostProcessor` itself. Nothing breaks the cycle: registering it as the feature readme documents (line 48, `AddScoped(typeof(IRequestPostProcessor<,>), typeof(MultiTimerPostProcessor<,>))`) gives **infinite recursion on the first dispatched action**. (No sample/test registers it today, consistent with it never having been exercised.) At minimum, every user action would pay a second full pipeline pass.

The deeper issue (finding 29): pipeline infrastructure has no general way to exclude internally-originated actions. `ActiveActionBehavior` (`action-tracking-behavior.cs:25`) hand-rolls the same exclusion with runtime `EnsureNotType` throws against two hard-coded action types — the failure mode for forgetting one is a runtime exception or infinite recursion, never a compile error.

## Fix

- Immediate: guard `MultiTimerPostProcessor` — `if (request is ResetTimersOnActivityActionSet.Action) return;`
- General: introduce an internal-action marker (e.g. `IInternalAction` on `ResetTimersOnActivityActionSet.Action`, `StartProcessingActionSet.Action`, `CompleteProcessingActionSet.Action`) honored by cross-cutting behaviors/post-processors, replacing `ActiveActionBehavior`'s hard-coded `EnsureNotType` list.

## Checklist

- [x] Recursion guard in MultiTimerPostProcessor + test (dispatching any action terminates; reset dispatched exactly once)
- [x] `IInternalAction` (or equivalent) marker + apply to the three internal actions
- [x] Replace `EnsureNotType` throws in ActiveActionBehavior with the marker check
- [x] Audit other open behaviors/processors for whether they should skip internal actions
- [x] Update timers feature readme registration guidance

## Notes

Coordinate the marker design with task 067 (render suppression), which wants a similar declarative, type-level mechanism.

`IInternalAction` is type identity ("the pipeline originated this for bookkeeping"), cached per closed generic with `typeof(IInternalAction).IsAssignableFrom(typeof(TRequest))`. Task 067 should keep render suppression as a separate attribute: Start/Complete processing must still re-render ActionTracking UI, and user actions may also want to skip render.

**Audit (who skips `IInternalAction`):**

- Skip: `MultiTimerPostProcessor` (recursion), `ActiveActionBehavior` (replaces `EnsureNotType`).
- Run: `RenderSubscriptionsPostProcessor` (tracking/timer UI), `StateTransactionBehavior`, `StateInitializationPreProcessor`, `PersistentStatePostProcessor`, `ReduxDevToolsBehavior`.
- Sample: `EventStreamBehavior` still type-skips `AddEventActionSet.Action`; left as test-app code.

## Session

- Created: code review 2026-06-11
- Implementer: grok session 01a0c62a-2617-77b1-a2b1-28b14675f72d (2026-09-22)

## Results

`IInternalAction` lives in TimeWarp.State and extends `IAction`. `ResetTimersOnActivityActionSet.Action`, `StartProcessingActionSet.Action`, and `CompleteProcessingActionSet.Action` implement it. `MultiTimerPostProcessor` skips internal requests after `next()`, so a nested `ResetTimersOnActivity` send does not re-enter the reset. `ActiveActionBehavior` skips tracking for `IInternalAction` even when `[TrackAction]` is also present; `ArgumentValidation.EnsureNotType` is gone. Timers feature readme registers the post-processor with `[assembly: MediatorBehavior(..., order: 550, Scope = typeof(ClientPipeline))]`.

**Files changed**

- `source/timewarp-state/state/i-internal-action.cs` (new)
- `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.reset-timers-on-activity.cs`
- `source/timewarp-state-plus/features/timers/readme.md`
- `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`
- `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.start-processing.cs`
- `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.complete-processing.cs`
- `tests/timewarp-state-plus-tests/features/timers/multi-timer-post-processor-tests.cs` (new)
- `tests/timewarp-state-plus-tests/features/action-tracking/active-action-behavior-tests.cs` (new)

**Decisions**

- Marker is an interface, not an attribute: identity of pipeline-originated work. Task 067 owns `[SuppressRender]`.
- Removing public `ArgumentValidation` is the replacement for the hard-coded throw list.
- Recursion tests drive `MultiTimerPostProcessor` with a recording `ISender<ClientPipeline>` (including a nested re-entry of the reset closed generic) rather than weaving the behavior into test-app.

**Tests**

- `dotnet fixie timewarp-state-plus-tests` — 31 passed, 1 skipped
- `dotnet fixie timewarp-state-tests` — 40 passed, 1 skipped
- `dotnet fixie client-integration-tests` — 42 passed, 1 skipped (includes `ActionTracking_Should`)
- `dotnet fixie test-app-architecture-tests` — 7 passed, 1 skipped

### How to validate

**Smoke**

```bash
dotnet fixie timewarp-state-plus-tests --tests MultiTimerPostProcessor_
dotnet fixie timewarp-state-plus-tests --tests ActiveActionBehavior_
```

**Expect**

- Exit 0.
- `MultiTimerPostProcessor_Should.Dispatch_Reset_Once_For_User_Request_And_Skip_On_Internal` — one `ResetTimersOnActivityActionSet.Action` send after a user request; handling that action does not send again.
- `MultiTimerPostProcessor_Should.Terminate_When_Nested_Dispatch_Runs_The_Same_Processor` — nested re-entry of the reset closed generic finishes within 2s with send count 1.
- `MultiTimerPostProcessor_Should.Skip_Reset_For_Start_And_Complete_Processing` — start/complete tracking actions send nothing.
- `ActiveActionBehavior_Should.Skip_Tracking_When_Action_Is_Internal` and `Skip_Tracking_When_Tracked_Action_Is_Also_Internal` — no Start/Complete sends.
- `ActiveActionBehavior_Should.Track_User_Action_With_Start_Then_Complete` — StartProcessing then CompleteProcessing around `next`.

**Automated gate**

```bash
dotnet fixie timewarp-state-plus-tests
# expect: 31 passed, 1 skipped

dotnet fixie client-integration-tests
# expect: 42 passed, 1 skipped; ActionTracking_Should still tracks [TrackAction] user actions
```

**Not in scope:** weaving `MultiTimerPostProcessor` into a sample host; Playwright; task 067 render suppression.
