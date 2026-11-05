# Round 1 — general
**Date:** 2026-11-05
**Scope reviewed:** branch `task/139-strengthen-and-split-the-four-500-loc-test-files-c` vs merge-base `1f7b34b5` (implementation `4056297f`), checked against `task.md` Requirements and `## Goal (implementer restatement)`.

## Summary

The four areas are split by the concerns the task names, and every file in those areas is under 400 lines (longest new file is 273). TWSG002 cases now assert the diagnostic id and message fragments, the duplicate PrivateNestedSubclass input is gone, the dictionary/Input guard checks both directions, the transaction harness takes options, overlap signals use `RunContinuationsAsynchronously` plus `WithTimeout`, analyzer spans are markup, and the new negatives and runtime clone shapes are present. One required assertion is still missing, and one new design comment names files this change deleted.

## Issues

### Issue 1 — Severity: bug
- File: tests/timewarp-state-tests/cloning/generated-clone-constructors-tests.cs:70
- Description: `Share_Injected_Services_By_Reference` still calls `clone.Logger.LogInformation("clone kept the logger")` and never asserts that a log was written. The logger is `NullLogger<ServiceState>.Instance`, so the call records nothing and the test still passes if the line is deleted. The task asks for an assertion on this generated-clone `LogInformation` call (the old line number was stale; the call lived in `generated-clone-shape-more-tests.cs`). The goal restatement puts that assertion with the runtime clone cases. The transaction test's `RecordingLogger` checks ("tick 0" and "tick 1") cover a different call, the one that used to sit inside the handler delegate.
- Suggestion: Give this clone a recording logger, call `LogInformation`, and assert the level and message after the clone, the same way `Keep_Injected_Services_Across_Two_Actions` asserts its ticks. Keep the existing reference-equality checks.
- Status: open

### Issue 2 — Severity: nit
- File: tests/timewarp-state-source-generator-tests/state-clone-shape-emitted-text-tests.cs:9
- Description: The Design region says runtime coverage lives in `generated-clone-shape-tests.cs` and `generated-clone-shape-more-tests.cs`. This change deleted both files. Runtime coverage is in `generated-clone-generics-tests.cs`, `generated-clone-collections-tests.cs`, and `generated-clone-constructors-tests.cs`.
- Suggestion: Point the comment at those three files.
- Status: open
