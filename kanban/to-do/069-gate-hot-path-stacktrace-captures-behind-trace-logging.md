# Gate hot-path StackTrace captures behind trace logging

## Description

Code review 2026-06-11, finding 17 (`code-review-2026-06-11.md`).

Three unconditional `new StackTrace()` captures (plus frame metadata resolution and string interpolation) sit on the hottest paths of every `TimeWarpStateComponent`:

- `ShouldRender` — `timewarp-state-component.cs:95`
- `SetParametersAsync` — `check-complex-parameter-changed.cs:35`
- `StateHasChanged` — `register-render-trigger.cs:49`

Inside the library the resulting `...WasCalledBy` strings are consumed only by `LogTrace` in `OnAfterRender`. Stack capture costs microseconds-to-milliseconds per call and is especially costly on Blazor WASM. Note: the properties are public and rendered by test-app diagnostic pages (`tests/test-app/test-app-client/pages/should-render-test-page/*.razor`), so gate rather than delete.

The three copy-pasted capture snippets have also already drifted: two record `Class.Method`, one records only the method name.

## Fix

- Guard each capture with `if (Logger.IsEnabled(LogLevel.Trace))` (or a `TimeWarpStateOptions` diagnostics flag, which would also keep the test-app diagnostic pages working by opting in).
- Extract one shared private helper (e.g. `GetCallerName()` wrapping the frame walk) so the three sites can't drift.
- Consider `[CallerMemberName]` plumbing where the caller is a known internal site — zero runtime cost and inlining-proof, unlike StackTrace in Release builds.

## Checklist

- [ ] Decide gating mechanism (log-level check vs options flag) — coordinate with test-app diagnostic pages
- [ ] Apply to all three sites via one shared helper
- [ ] Verify test-app should-render diagnostic pages still show data when the gate is enabled
