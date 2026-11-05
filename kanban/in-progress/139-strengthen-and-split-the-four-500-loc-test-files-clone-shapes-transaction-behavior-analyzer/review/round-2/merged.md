# Round 2 — merged findings
**Date:** 2026-11-05
**Sources:** general
**Scope:** fix commit `e0fed5e6` since round-1 base `e760fb79`

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tests/timewarp-state-tests/cloning/generated-clone-constructors-tests.cs:72
- Description: Round 1: `Share_Injected_Services_By_Reference` called `LogInformation` on a `NullLogger` and never asserted a log entry. Re-checked on `e0fed5e6`: the test uses `RecordingLogger<ServiceState>`, asserts `clone.Logger.ShouldBeSameAs(logger)`, then asserts one Information entry whose message is `"clone kept the logger"`.
- Suggestion: none remaining
- Source: general
- Disposition notes: fixed in `e0fed5e6`. Verified on the fix delta in round 2. `RecordingLogger` now lives in `tests/timewarp-state-tests/shared/recording-logger.cs` (`SharedTestFixtures`). Pipeline call sites resolve through the existing global using.

### M2 — Severity: nit — Status: fixed
- File: tests/timewarp-state-source-generator-tests/state-clone-shape-emitted-text-tests.cs:9
- Description: Round 1: the Design comment named deleted runtime files. Re-checked on `e0fed5e6`: it names `generated-clone-generics-tests.cs`, `generated-clone-collections-tests.cs`, and `generated-clone-constructors-tests.cs`, which are the runtime tests for those shapes.
- Suggestion: none remaining
- Source: general
- Disposition notes: fixed in `e0fed5e6`. Verified on the fix delta in round 2.

## Duplicates / conflicts

- Single reviewer. No new findings. Prior M1 and M2 stay fixed.
