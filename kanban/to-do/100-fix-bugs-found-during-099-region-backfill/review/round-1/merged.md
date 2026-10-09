# Round 1 — merged findings
**Date:** 2026-10-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state/features/pipeline/invalid-clone-exception.cs:48-53
- Description: The EmptyGuid message tells the user to "use the default cloner". But the default cloner itself returns an uninitialized instance with an empty Guid when the state's constructor throws or none can be called (deep-cloner.cs:384-398).
- Suggestion: Reword the message to cover the default-cloner fallback, and add a test with a non-ICloneable state whose constructor throws.
- Source: general (orchestrator checked it against deep-cloner.cs)
- Disposition notes: Fixed in fb84482e. The EmptyGuid message covers both the custom-ICloneable and default-cloner paths; new test Throw_InvalidCloneException_When_Default_Cloner_Constructor_Throws.

### M2 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release12.0.0-beta.10.md:13; source/timewarp-state-plus/state/timewarp-cacheable-state.cs:9-10
- Description: "A declaration that passed any other type no longer compiles" overclaims. `A : TimeWarpCacheableState<B>`, where B is another cacheable state, still compiles, and the analyzer does not check indirect subclasses.
- Suggestion: Reword the release note, migration doc and Design region to say what is actually enforced.
- Source: general
- Disposition notes: Fixed in fb84482e. The release note, migration guide and Design region say what the constraint and analyzer enforce. The analyzer is unchanged.

### M3 — Severity: suggestion — Status: fixed
- File: tests/test-app/test-app-server/program.cs:72-80
- Description: The new server throw endpoint has no test that would fail before the fix.
- Suggestion: Add a client-integration test asserting a 500 response that carries the message.
- Source: general
- Disposition notes: Fixed in fb84482e. New throw-server-side-exception-endpoint-tests.cs asserts a 500 response whose body contains the message.

### M4 — Severity: nit — Status: fixed
- File: tests/test-app/test-app-server/program.cs:69
- Description: `int days` is required, so a plain `api/weather` request returns 400.
- Suggestion: Default it to 5.
- Source: general
- Disposition notes: Fixed in fb84482e. days defaults to 5; new weather-endpoint-tests.cs checks that a plain api/weather returns 5 forecasts.

## Duplicates / conflicts

- None.
