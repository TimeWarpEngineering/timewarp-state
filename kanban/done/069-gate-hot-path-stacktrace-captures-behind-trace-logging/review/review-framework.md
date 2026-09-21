# Review framework — task 069

**Date:** 2026-09-21
**Host task:** kanban/in-progress/069-gate-hot-path-stacktrace-captures-behind-trace-logging/
**Diff scope:** branch `task/069-gate-hot-path-stacktrace-captures-behind-trace-log` vs `origin/master` (implement commit `fdc1198f`; kanban results `6546b6a3`)
**Plan / brief:** Code review 2026-06-11 finding 17 (still true after 073). Gate three unconditional `new StackTrace()` captures on every `TimeWarpStateComponent` behind `TimeWarpStateOptions.CaptureRenderCaller` (default false). Do not key off `Logger.IsEnabled(LogLevel.Trace)` — diagnostic pages need the strings on screen. Test-app opts in. Shared `Class.Method` helper; keep `GetFrame(1)` at each site. Flag off → skip StackTrace, leave `*WasCalledBy` null. Keep public properties.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c3de-0f4c-7271-93ff-ed090c368d71` (2026-09-21)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/extensions/timewarp-state-options.cs`
- `source/timewarp-state/components/timewarp-state-component.capture-render-caller.cs` (new)
- `source/timewarp-state/components/timewarp-state-component.cs`
- `source/timewarp-state/components/timewarp-state-component.check-complex-parameter-changed.cs`
- `source/timewarp-state/components/timewarp-state-component.register-render-trigger.cs`
- `tests/test-app/test-app-client/program.cs`
- `tests/timewarp-state-tests/timewarp-state-component/capture-render-caller-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`

## Requirements to check

- `TimeWarpStateOptions.CaptureRenderCaller` defaults to false
- Do **not** gate only on `Logger.IsEnabled(LogLevel.Trace)`
- Test-app / diagnostic pages opt in (host setup sets the flag true; server host reuses client `ConfigureServices`)
- One shared helper for `Class.Method` at all three sites (`ShouldRender`, `SetParametersAsync`, `StateHasChanged`)
- `[CallerMemberName]` is not used; each site still uses `GetFrame(1)` so the frame is the invoker
- Flag off: skip `StackTrace`; `*WasCalledBy` stay null; public properties kept
- Tests: default false; flag off → properties null; flag on → `Class.Method` at all three sites
- Out of scope: Redux DevTools `BuildStackTrace`; deleting diagnostic pages; render-reason boolean flags
