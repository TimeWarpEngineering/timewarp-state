# Round 2 — merged findings
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
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:60
- Description: Span name and `timewarp.state.action` used `typeof(TRequest).Name`, which is always `Action` under `XxxActionSet.Action`.
- Suggestion: Nested `DeclaringType` display name in static readonly fields.
- Source: general (round 1)
- Disposition notes: Activity name is the full nested chain; action tag is relative to the enclosing state. Test `Emit_Nested_ActionSet_Display_Name`. Round 2 confirmed.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-state-telemetry/assembly-marker.cs:16
- Description: Order 50 sat outside `StateTransactionBehavior`, which swallows handler exceptions, so spans were `Ok` on handler failure.
- Suggestion: Weave at order 350 (inside transaction 300, outside render 400).
- Source: general (round 1)
- Disposition notes: `order: 350`. Swallowing outer still records Error. Attribute test plus README composition note. Round 2 confirmed.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-telemetry/telemetry-behavior.cs:189
- Description: Truncation ran before cache compare, hiding diffs and emitting invalid JSON as the compare key.
- Suggestion: Compare full JSON; truncate event payload only; `snapshot.truncated` tag.
- Source: general (round 1)
- Disposition notes: Cache gets full JSON. Event payload truncated with `snapshot.truncated`. Test `Truncate_Event_Payload_After_Cache_Compare`. Round 2 confirmed.

## Duplicates / conflicts

- None. Round 2 raised no new findings. Prior M# IDs carried forward.
