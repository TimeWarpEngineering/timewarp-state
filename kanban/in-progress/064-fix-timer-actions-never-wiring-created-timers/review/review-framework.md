# Review framework — task 064

**Date:** 2026-09-20
**Host task:** kanban/in-progress/064-fix-timer-actions-never-wiring-created-timers/
**Diff scope:** branch `task/064-fix-timer-actions-never-wiring-created-timers` vs `origin/master` (product commit `938e1700`)
**Plan / brief:** Code-review 2026-06-11 finding 4. Add/Update stored a bare `new Timer(duration)` with no `Elapsed`, `AutoReset = false`, or `Start()`. Extract shared `CreateTimer` and use it from Initialize / AddTimer / UpdateTimer. Dispose replaced timers. Prove an action-created timer publishes `TimerElapsedNotification`. Re-implement closed PR #570 on current `ValueTask` + `IPublisher<ClientPipeline>` APIs; do not merge that branch.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle (2026-09-20)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state-plus/features/timers/timer-state/timer-state.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.add-timer.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.update-timer.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.remove-timer.cs`
- `tests/timewarp-state-plus-tests/features/timers/add-timer-tests.cs` (new)

Surrounding call sites (not modified, still in review scope for regressions):

- `source/timewarp-state-plus/features/timers/timer-state/timer-state.reset-timers-on-activity.cs`
- `source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs`
- `source/timewarp-state-plus/features/timers/timer-elapsed-notification.cs`

## Requirements to check

- `CreateTimer` is the only constructor path: attach `Elapsed` → `OnTimerElapsed`, `AutoReset = false`, `Start()`, then store
- `Initialize`, `AddTimerActionSet.Handler`, and `UpdateTimerActionSet.Handler` all call `CreateTimer`
- `UpdateTimer` Stop+Disposes the timer it replaces (same-name replace inside `CreateTimer`)
- `RemoveTimer` and `TimerState.Dispose` Stop+Dispose live timers
- `UpdateTimer` remains a no-op when the name is missing
- Test: timer added via action publishes `TimerElapsedNotification` after its duration
- Handlers stay `ValueTask` + current mediator types (not pre-080 `Task Handle`)
- MultiTimerPostProcessor recursion remains out of scope (task 066)
