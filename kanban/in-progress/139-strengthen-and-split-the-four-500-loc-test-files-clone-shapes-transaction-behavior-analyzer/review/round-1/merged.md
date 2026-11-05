# Round 1 — merged findings
**Date:** 2026-11-05
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tests/timewarp-state-tests/cloning/generated-clone-constructors-tests.cs:70
- Description: `Share_Injected_Services_By_Reference` still calls `clone.Logger.LogInformation("clone kept the logger")` and never asserts that a log was written. The logger is `NullLogger<ServiceState>.Instance`, so the call records nothing and the test still passes if the line is deleted. The task asks for an assertion on this generated-clone `LogInformation` call (the old line number was stale; the call lived in `generated-clone-shape-more-tests.cs`). The goal restatement puts that assertion with the runtime clone cases. The transaction test's `RecordingLogger` checks ("tick 0" and "tick 1") cover a different call, the one that used to sit inside the handler delegate.
- Suggestion: Give this clone a recording logger, call `LogInformation`, and assert the level and message after the clone, the same way `Keep_Injected_Services_Across_Two_Actions` asserts its ticks. Keep the existing reference-equality checks.
- Source: general
- Disposition notes: fixed. The test now uses a shared `RecordingLogger<ServiceState>` (moved to `shared/recording-logger.cs`) and asserts one Information entry "clone kept the logger" after the clone's `LogInformation` call; reference checks kept.

### M2 — Severity: nit — Status: fixed
- File: tests/timewarp-state-source-generator-tests/state-clone-shape-emitted-text-tests.cs:9
- Description: The Design region says runtime coverage lives in `generated-clone-shape-tests.cs` and `generated-clone-shape-more-tests.cs`. This change deleted both files. Runtime coverage is in `generated-clone-generics-tests.cs`, `generated-clone-collections-tests.cs`, and `generated-clone-constructors-tests.cs`.
- Suggestion: Point the comment at those three files.
- Source: general
- Disposition notes: fixed. The comment now names generated-clone-generics-tests.cs, generated-clone-collections-tests.cs, and generated-clone-constructors-tests.cs.

## Duplicates / conflicts

- Single reviewer. Nothing to collapse.
