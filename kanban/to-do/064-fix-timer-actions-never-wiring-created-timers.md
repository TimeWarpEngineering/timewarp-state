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

- [ ] Extract shared CreateTimer helper
- [ ] Use it in Initialize / AddTimer handler / UpdateTimer handler
- [ ] Dispose the replaced timer in UpdateTimer (audit RemoveTimer/Dispose paths too)
- [ ] Test: timer added via action publishes `TimerElapsedNotification` after its duration
