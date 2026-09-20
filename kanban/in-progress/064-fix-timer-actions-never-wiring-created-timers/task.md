# Fix timer actions never wiring created timers

## Description

Code review 2026-06-11, finding 4 (`code-review-2026-06-11.md`).

`source/timewarp-state-plus/features/timers/timer-state/timer-state.add-timer.cs:27` and `timer-state.update-timer.cs:32–33` store a bare `new Timer(duration)` without attaching `Elapsed`, without `AutoReset = false`, and without `Start()`. `TimerState.Initialize` (`timer-state.cs:55–58`) does all three:

```csharp
timer.Elapsed += (_, _) => OnTimerElapsed(timerName);
timer.AutoReset = false;
timer.Start();
```

Nothing wires the handler later — `RestartTimer` (`timer-state.cs:83–84`) only calls `Stop(); Start()`. Any timer added via `AddTimerActionSet` or replaced via `UpdateTimerActionSet` silently never publishes `TimerElapsedNotification` (and if started later, fires with default `AutoReset = true`, unlike configured timers).

## Fix

Extract Initialize's wiring into a shared private `CreateTimer(string timerName, TimerConfig config)` helper on `TimerState` and use it from `Initialize`, the `AddTimer` handler, and the `UpdateTimer` handler. `UpdateTimer` should also stop/dispose the timer it replaces.

## Checklist

- [x] Extract shared CreateTimer helper
- [x] Use it in Initialize / AddTimer handler / UpdateTimer handler
- [x] Dispose the replaced timer in UpdateTimer (audit RemoveTimer/Dispose paths too)
- [x] Test: timer added via action publishes `TimerElapsedNotification` after its duration
- [x] Implementation review disposition (same task id)

## Notes

[PR #570](https://github.com/TimeWarpEngineering/timewarp-state/pull/570) (@nhwilly, closed 2026-09-20) is the same approach (`CreateAndStartTimer` from Initialize / Add / Update; Stop+Dispose on replace/remove). Do **not** merge that branch — it is pre-080 (`Task Handle`, old mediator types). Current handlers are `ValueTask` + `IPublisher<ClientPipeline>`. Re-implement on this worktree; credit the idea.

`Initialize` currently wires Elapsed/AutoReset/Start inline. Add/Update still `new Timer(duration)` only.

2026-09-20: implemented on this worktree (see Results); did not merge #570.

## Session

- Created: code review 2026-06-11
- 2026-09-20: cockpit closed #570 in favor of this id; dispatching implementer-grok
- Implementer: grok (2026-09-20) — re-implemented #570's helper on ValueTask / IPublisher<ClientPipeline>
- Review oracle: grok (2026-09-20) — tw-implementation-review effort 1, roster general (grok-4.5 subagent, read-only); disposition clean

## Results

Action-created timers now take the same path as option-seeded ones. `TimerState.CreateTimer` is the only constructor: attach `Elapsed` → `OnTimerElapsed`, `AutoReset = false`, `Start()`, then store. `Initialize`, `AddTimerActionSet.Handler`, and `UpdateTimerActionSet.Handler` all call it. Same-name replace Stop+Disposes the previous `System.Timers.Timer` inside `CreateTimer`; `RemoveTimer` and `TimerState.Dispose` use the same `StopAndDispose` helper. Approach credited to closed [PR #570](https://github.com/TimeWarpEngineering/timewarp-state/pull/570) (@nhwilly); handlers stay `ValueTask` + `IPublisher<ClientPipeline>` (not the pre-080 `Task Handle`).

**Files changed**

- `source/timewarp-state-plus/features/timers/timer-state/timer-state.cs` — `CreateTimer` / `StopAndDispose`; Initialize and Dispose walk them
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.add-timer.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.update-timer.cs`
- `source/timewarp-state-plus/features/timers/timer-state/timer-state.remove-timer.cs`
- `tests/timewarp-state-plus-tests/features/timers/add-timer-tests.cs`

**Decisions**

- Helper name is `CreateTimer` as specified (PR #570 used `CreateAndStartTimer`).
- `UpdateTimer` remains a no-op when the name is missing; `CreateTimer` only runs after `ContainsKey`.
- `Initialize` Stop+Disposes any live timers before `Clear()` so re-init does not leak.

**Tests:** `dotnet fixie timewarp-state-plus-tests` — 19 passed, 1 skipped.

### How to validate

**Smoke**

```bash
dotnet fixie timewarp-state-plus-tests
```

**Expect**

- Exit 0.
- Cases under `AddTimer_.AddTimer_Should`:
  - `Publish_TimerElapsedNotification_When_Added_Via_Action` — an `AddTimer` action with a 50ms duration publishes `TimerElapsedNotification` named `SessionTimer`.
  - `Not_Publish_After_Timer_Is_Removed` — remove before the duration elapses; notification count stays 0.
  - `Not_Publish_Original_Duration_After_Update` — replace a 50ms timer with 30s; waiting past 50ms does not publish (old timer was Stop+Disposed).
- Suite summary: `19 passed, 1 skipped` (skip is the existing convention demo).

**Automated gate**

```bash
dotnet build ./tests/timewarp-state-plus-tests/timewarp-state-plus-tests.csproj
dotnet fixie timewarp-state-plus-tests
```

**Not in scope:** `MultiTimerPostProcessor` recursion (task 066). No sample host registers the post-processor.

### Review disposition

**Outcome:** `clean` (0 open findings; no `wontfix`)
**Effort:** 1 (general only)
**Rounds:** 1
**Roster:** general (`review/round-1/general.md`)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

Round 1 found no issues. No fix loop. No escalations. Merge pass confirmed `CreateTimer` is the only constructor, Add/Update/Initialize share it, replace/remove/Dispose Stop+Dispose, and `dotnet fixie timewarp-state-plus-tests` is 19 passed, 1 skipped.

**Review paths**

- `kanban/in-progress/064-fix-timer-actions-never-wiring-created-timers/review/review-framework.md`
- `kanban/in-progress/064-fix-timer-actions-never-wiring-created-timers/review/round-1/general.md`
- `kanban/in-progress/064-fix-timer-actions-never-wiring-created-timers/review/round-1/merged.md`
- `kanban/in-progress/064-fix-timer-actions-never-wiring-created-timers/review/disposition.md`
