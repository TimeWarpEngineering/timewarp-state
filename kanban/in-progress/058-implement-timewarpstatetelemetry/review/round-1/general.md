# Round 1 — general
**Date:** 2026-09-22
**Scope reviewed:** branch `task/058-implement-timewarpstatetelemetry` vs `origin/master`

## Summary

`TimeWarp.State.Telemetry` is a packable, AOT-flagged ClientPipeline behavior that emits one `ActivitySource` (`TimeWarp.State`) span per `IAction`, with opt-in JSON snapshots as span events behind `HasListeners` / `StartActivity` / `IsAllDataRequested` / `IncludeSnapshots` / caller `JsonTypeInfo`. Hot path, TryAdd registration, sample OTLP wiring, package README (WASM, security, performance), and the isolated Fixie suite match the brief. Residual risk is in production observability identity: standard `XxxActionSet.Action` names collapse, and default `StateTransactionBehavior` swallows handler exceptions before the order-50 behavior can set Error.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:33
- Description: Span name and `timewarp.state.action` both use `typeof(TRequest).Name`. TimeWarp.State’s convention (every sample and the test-app) is `State.XxxActionSet.Action`, so `.Name` is always `Action`. A state with Increment and Decrement is two spans named `CounterState.Action` with tag `timewarp.state.action=Action`. Tests hide this by using `IncrementAction` / `ThrowAction` instead of `ActionSet.Action`. Redux DevTools already distinguishes actions via `request.GetType().FullName`.
- Suggestion: Cache a nested display name from the `DeclaringType` chain (e.g. `CounterState.IncrementCountActionSet.Action`) in the existing `static readonly` fields — still `typeof` only, no `AssemblyQualifiedName` / `Type.GetType`. Put that string in the span name and the action tag. Add a test whose request type is `TelemetryTestState.IncrementCountActionSet.Action`.
- Status: open

### Issue 2 — Severity: bug
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:121
- Description: Error status + rethrow is implemented and unit-tested, but only when `next()` throws. In the woven pipeline, `TelemetryBehavior` is order 50 (outermost) and `StateTransactionBehavior` is order 300 with `UseStateTransactionBehavior` defaulting true. On handler failure the transaction restores state and `return default!` without rethrow, so telemetry always `SetStatus(Ok)` and may snapshot the rolled-back state. Clone failures (thrown *before* the transaction `try`) would still surface as Error; handler failures in a normal host would not. The sample has no failing action, so the Aspire timeline never demonstrates Error.
- Suggestion: Weave inside the transaction (order between 300 and 400, e.g. 350) so handler exceptions hit telemetry’s `catch` before they are swallowed; duration still covers handler + render. Or, if order 50 stays for full-pipeline duration, treat `ExceptionNotification` (or an explicit failure signal) as Error. Add a test that runs `TelemetryBehavior` around `StateTransactionBehavior` (or a next() that swallows like it) and assert Error vs Ok. Document whichever composition you keep.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:189
- Description: JSON is truncated (`MaxSnapshotChars` + `…(truncated)`) *before* `StateSnapshotCache.Record`. Two payloads that differ only after the cap compare equal, so no `state.diff`. The event tag `snapshot.json` is also no longer valid JSON, which will break any consumer that parses it. README correctly says truncation is not redaction; it does not say it mutates the diff key or invalidates JSON.
- Suggestion: Cache and compare the full string; truncate only the event payload (and add a boolean tag such as `snapshot.truncated`). Keep the size cap on what is exported, not on what “changed” means.
- Status: open
