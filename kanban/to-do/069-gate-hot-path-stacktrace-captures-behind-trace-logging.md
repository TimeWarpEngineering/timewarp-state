# Gate hot-path StackTrace behind TimeWarpStateOptions

## Description

Code review 2026-06-11, finding 17. Still true after 073.

Three unconditional `new StackTrace()` captures on every `TimeWarpStateComponent`:

- `ShouldRender` — `timewarp-state-component.cs` (~97)
- `SetParametersAsync` — `check-complex-parameter-changed.cs` (~35) — method name **only**
- `StateHasChanged` — `register-render-trigger.cs` (~48) — `Class.Method`

Consumed by test-app diagnostic pages (`should-render-test-page`, `should-render-state-triggers-test-page`). Gate, do not delete. The three snippets have drifted.

## Requirements

- **`TimeWarpStateOptions` flag** (e.g. `CaptureRenderCaller`), default **false**. Do **not** use only `Logger.IsEnabled(LogLevel.Trace)` — diagnostic pages need the strings on screen.
- Test-app / those pages **opt in** (set the flag true in host setup).
- One shared helper for `Class.Method` (same format at all three sites). `[CallerMemberName]` cannot replace `GetFrame(1)` for “who called ShouldRender.”
- When the flag is off: skip `StackTrace`; leave `*WasCalledBy` **null** (pages already render empty).
- Keep the public properties.
- Tests: flag off → no capture (or at least properties stay null); flag on → `Class.Method` at all three sites.

## Out of scope

- Redux DevTools `BuildStackTrace` (already gated)
- Deleting diagnostic pages
- Render-reason boolean flags (`ShouldRenderWasCalled` etc.)

## Checklist

- [ ] Options flag default false; test-app opts in
- [ ] Shared helper; three sites consistent
- [ ] Diagnostic pages still show caller when opted in
- [ ] `dev test` green

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk — options flag not Trace-only; dispatch implementer-grok
