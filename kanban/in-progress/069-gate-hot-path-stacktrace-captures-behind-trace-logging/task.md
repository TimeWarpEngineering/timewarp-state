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

- [x] Options flag default false; test-app opts in
- [x] Shared helper; three sites consistent
- [x] Diagnostic pages still show caller when opted in
- [x] `dev test` green
- [x] Review round 1 (general, effort 1); disposition clean

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk — options flag not Trace-only; dispatch implementer-grok
- Implementer: grok session (2026-09-21)
- Review: grok oracle `01a0c3de-0f4c-7271-93ff-ed090c368d71` (2026-09-21) — effort 1 general; round 1; disposition clean

## Results

Hot-path `new StackTrace()` on every `TimeWarpStateComponent` is gated behind `TimeWarpStateOptions.CaptureRenderCaller` (default **false**). Diagnostic pages still get `Class.Method` on screen because the test-app host sets the flag true — not via `Logger.IsEnabled(LogLevel.Trace)`.

**What landed**

- `TimeWarpStateOptions.CaptureRenderCaller` defaults to false.
- Shared `FormatRenderCaller` formats `Class.Method` at all three sites (`ShouldRender`, `SetParametersAsync`, `StateHasChanged`). Each site still uses `GetFrame(1)` so the frame is the invoker, not the TimeWarp method. `SetParametersAsync` no longer records method name only.
- Flag off: skip `StackTrace`; `*WasCalledBy` stay null. Public properties kept.
- Test-app `ConfigureServices` sets `options.CaptureRenderCaller = true` (server host reuses that).
- Unit tests: default false; flag off → properties null; flag on → `Class.Method` at all three sites.

**Files**

- `source/timewarp-state/extensions/timewarp-state-options.cs`
- `source/timewarp-state/components/timewarp-state-component.capture-render-caller.cs` (new)
- `source/timewarp-state/components/timewarp-state-component.cs`
- `source/timewarp-state/components/timewarp-state-component.check-complex-parameter-changed.cs`
- `source/timewarp-state/components/timewarp-state-component.register-render-trigger.cs`
- `tests/test-app/test-app-client/program.cs`
- `tests/timewarp-state-tests/timewarp-state-component/capture-render-caller-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`

**Decisions**

- Options flag, not Trace-only: diagnostic pages render the strings on screen.
- Format helper is shared; capture stays at each call site with `GetFrame(1)` so `[CallerMemberName]` is not used.
- Redux DevTools `BuildStackTrace` and render-reason booleans untouched.

**Review**

- Effort 1; roster: general; rounds: 1
- Final counts: bug 0/0/0 open/fixed/wontfix; suggestion 0; nit 0
- **Disposition: clean** (no issues raised; no fix loop)
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`

**Tests**

`dotnet run --file ./scripts/test.cs` exit 0:

- analyzer 19 passed
- source generator 4 passed
- state 24 passed, 1 skipped (includes the five new capture tests)
- plus 25 passed, 1 skipped
- client integration 42 passed, 1 skipped
- architecture 7 passed, 1 skipped

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests
```

**Expect**

- Exit 0.
- `CaptureRenderCaller.Should_.Options_Default_CaptureRenderCaller_Is_False` passes.
- `Flag_Off_Leaves_Caller_Properties_Null` passes (`*WasCalledBy` stay null).
- The three `Flag_On_Captures_Class_Method_From_*` tests pass with `TestRenderCallerComponent.Trigger…` (`Class.Method`, not method name only).

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; all suites pass (analyzer, generator, state, plus, client integration, architecture)
```

**Manual (diagnostic pages)**

```bash
dotnet run --file ./scripts/run-test-app.cs
# open http://localhost:5058/should-render-test
# open http://localhost:5058/render-triggers-test
# expect: ShouldRenderWasCalledBy / SetParametersAsyncWasCalledBy / StateHasChangedWasCalledBy show Class.Method after a render, not empty
```

**Not in scope:** Redux DevTools `BuildStackTrace`; deleting diagnostic pages; render-reason boolean flags.
