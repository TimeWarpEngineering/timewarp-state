# Round 1 — merged findings
**Date:** 2026-09-22
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:33
- Description: Span name and `timewarp.state.action` both use `typeof(TRequest).Name`. TimeWarp.State’s convention is `State.XxxActionSet.Action`, so `.Name` is always `Action`. A state with Increment and Decrement is two spans named `CounterState.Action` with tag `timewarp.state.action=Action`. Tests hide this by using `IncrementAction` / `ThrowAction` instead of `ActionSet.Action`. Redux DevTools already distinguishes actions via `request.GetType().FullName`.
- Suggestion: Cache a nested display name from the `DeclaringType` chain (e.g. `CounterState.IncrementCountActionSet.Action`) in the existing `static readonly` fields — still `typeof` only, no `AssemblyQualifiedName` / `Type.GetType`. Put that string in the span name and the action tag. Add a test whose request type is `TelemetryTestState.IncrementCountActionSet.Action`.
- Source: general
- Disposition notes: Static ctor walks `DeclaringType` (typeof only). Activity name is the full nested chain (`TelemetryTestState.IncrementCountActionSet.Action`); `timewarp.state.action` is relative to the enclosing state (`IncrementCountActionSet.Action`); `timewarp.state.state_type` stays the state name. Direct nested `IncrementAction` still emits `TelemetryTestState.IncrementAction` / tag `IncrementAction`. Test `Emit_Nested_ActionSet_Display_Name` covers `ActionSet.Action`. Docs updated off `CounterState.Action`.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:121
- Description: Error status + rethrow is implemented and unit-tested, but only when `next()` throws. In the woven pipeline, `TelemetryBehavior` is order 50 (outermost) and `StateTransactionBehavior` is order 300 with `UseStateTransactionBehavior` defaulting true. On handler failure the transaction restores state and `return default!` without rethrow, so telemetry always `SetStatus(Ok)` and may snapshot the rolled-back state. Clone failures (thrown *before* the transaction `try`) would still surface as Error; handler failures in a normal host would not.
- Suggestion: Weave inside the transaction (order between 300 and 400, e.g. 350) so handler exceptions hit telemetry’s `catch` before they are swallowed; duration still covers handler + render. Add a test that runs `TelemetryBehavior` inside a next() that swallows like `StateTransactionBehavior` and assert Error. Document the composition. Prove weave order (attribute `order: 350`).
- Source: general
- Disposition notes: Weave order is 350 (`[assembly: MediatorBehavior(..., order: 350, ...)]`), between transaction 300 and render 400. Test `Record_Error_When_Transaction_Swallows_Handler_Exception` wraps `Handle` in a swallowing try/catch and asserts `ActivityStatusCode.Error`. Test `Weave_Telemetry_Inside_State_Transaction_At_Order_350` reads `CustomAttributeData` on `typeof(TimeWarp.State.Telemetry.AssemblyMarker).Assembly`. Package README documents that handler failures are Error and duration is handler+render, not clone/Redux JS.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:189
- Description: JSON is truncated (`MaxSnapshotChars` + `…(truncated)`) *before* `StateSnapshotCache.Record`. Two payloads that differ only after the cap compare equal, so no `state.diff`. The event tag `snapshot.json` is also no longer valid JSON.
- Suggestion: Cache and compare the full string; truncate only the event payload (and add a boolean tag such as `snapshot.truncated`). Keep the size cap on what is exported, not on what “changed” means.
- Source: general
- Disposition notes: `StateSnapshotCache.Record` gets the full JSON. Truncation applies only to the event payload. Events include boolean tag `snapshot.truncated`. Test `Truncate_Event_Payload_After_Cache_Compare` uses `MaxSnapshotChars = 40` with Count 1 then 2 so the truncated prefixes match; the second dispatch still emits `state.diff` with a truncated payload and `snapshot.truncated` true.

## Duplicates / conflicts

- None. Three distinct findings from general.
