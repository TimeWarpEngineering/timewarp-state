# Round 1 — general-telemetry
**Date:** 2026-10-12
**Scope reviewed:** telemetry + core + samples + telemetry tests

## Summary
The `TelemetryBehavior` extraction into `StateSnapshotRecorder` is behaviour-equivalent: check order, cache key, snapshot-vs-diff selection, truncation, and the missing-JsonTypeInfo skip log are unchanged. `InitializedStateSnapshotHandler` is opt-in, never throws into the store path, and the Mediator generator does register it (scoped, wired into the generated `StateInitializedNotification` publisher). `dotnet test tests/timewarp-state-telemetry-tests` passes (17/17). No bugs found; only test-coverage gaps and nits.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-state-telemetry-tests/initialized-state-snapshot-handler-tests.cs:1
- Description: Init-path tests do not cover `MaxSnapshotChars` truncation / `snapshot.truncated` on the `.Initialized` span, the serialization-failure path (activity status Error "snapshot failed"), or that the handler is actually wired in DI (`AddTimeWarpStateTelemetry` + generated mediator, real `Store` publishing `StateInitializedNotification`). Wiring was only verified by inspecting generated code, not by a test.
- Suggestion: Add a truncation test and a throwing-serializer test; optionally an integration test with a real Store asserting an `X.Initialized` span arrives before the first action span.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-state-telemetry/state-snapshot-recorder.cs:62
- Description: In `TryRecord`, `cache.Record(...)` sits outside any try/catch, whereas before the refactor it was inside the snapshot try block. `Record` only throws for a null/empty key, and the key is `FullName ?? Name`, so this is unreachable in practice, but the "Does not throw" doc comment is slightly stronger than the code.
- Suggestion: Move `cache.Record` into the same guarded region or soften the doc comment.
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-state-telemetry/initialized-state-snapshot-handler.cs:85
- Description: The handler duplicates the options / `JsonTypeInfo` resolve and skip logging that `TryCaptureJson` already does (to avoid starting an empty span). It works and is not double-logged, but the two copies of the log text can drift.
- Suggestion: Add a `CanSerialize(...)` helper on the recorder that both use, or leave as is.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state-telemetry/initialized-state-snapshot-handler.cs:125
- Description: When the init activity is sampled out (`activity is null` or `!IsAllDataRequested`), the cache is not primed. The first action then emits `state.snapshot`, not `state.diff`. This is consistent and harmless (full JSON either way), but unlike the other paths it is undocumented.
- Suggestion: Note it in the Design region, or leave as is.
- Status: open
