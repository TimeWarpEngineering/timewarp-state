# Round 1 — merged findings
**Date:** 2026-10-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 3 | 0 |

## Issues

### M1 — Severity: nit — Status: fixed
- File: source/timewarp-state/features/javascript-interop/json-request-handler.cs:66
- Description: An empty name threw `ArgumentException` and logged no warning.
- Suggestion: Log a warning and throw `InvalidRequestTypeException`.
- Source: general
- Disposition notes: Fixed. An empty name now logs a `JsonRequestOfInvalidType` warning and throws `InvalidRequestTypeException`. Test: `Reject_An_Empty_Name`.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state/features/javascript-interop/json-request-handler.cs:128
- Description: Warning logs carried the caller-supplied JSON in full, and the raw name without a length cap.
- Suggestion: Keep the payload at Debug only, and truncate the name.
- Source: general
- Disposition notes: Fixed. The invalid-payload warning logs only the type FullName and the exception. The unknown-name warning truncates the name to 256 chars. The JSON is still logged at Debug only.

### M3 — Severity: nit — Status: fixed
- File: source/timewarp-state/features/javascript-interop/json-request-handler.cs:113
- Description: The narrow catch filter let constructor or setter exceptions escape without a warning.
- Suggestion: Broaden the catch.
- Source: general
- Disposition notes: Fixed. The catch is now `when (exception is not OutOfMemoryException)`, and the inner exception is kept. Test: `Reject_A_Payload_Whose_Setter_Throws`.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-state/features/javascript-interop/javascript-dispatch-registry.cs:60
- Description: A failed alias left the type partly registered.
- Suggestion: Validate before mutating.
- Source: general
- Disposition notes: Fixed. `Add` validates the alias and every name before it writes any. The Design region now notes that the registry is written at startup and read-only afterwards. Test: `Refuse_An_Alias_That_Collides_And_Leave_The_Registry_Unchanged`.

### M5 — Severity: suggestion — Status: fixed
- File: tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs
- Description: Tests did not cover the version-mismatched AQN, alias collision, `null` JSON or the empty name.
- Suggestion: Add tests for these cases.
- Source: general
- Disposition notes: Fixed. The version-mismatched AQN (`IncrementWireName`, Version=9.9.9.9) and the `"null"` payload (`Reject_Bad_Json`) were already covered. Added tests for the empty name, the alias collision, and the throwing setter.

## Duplicates / conflicts

- None.
