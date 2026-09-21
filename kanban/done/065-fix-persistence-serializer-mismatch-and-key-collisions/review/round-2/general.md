# Round 2 — general
**Date:** 2026-09-21
**Scope reviewed:** fix delta for M1/M2 on branch `task/065-fix-persistence-serializer-mismatch-and-key-collis`

## Summary

Re-checked the uncommitted fix delta against current product code. Load now deserializes with a ctor-time clone of `TimeWarpStateOptions.JsonSerializerOptions` that sets `PropertyNameCaseInsensitive = true`, and `Load_Falls_Back_To_PascalCase_Name_Key` seeds Blazored-shaped PascalCase JSON under the simple Name key. Save still serializes with the shared CamelCase options, and serialization runs only inside `WriteAsync` after Session/Local null-storage checks, so optional-Blazored skip and Server/PreRender no-ops avoid serialize cost. No new defects in the scoped delta.

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-plus/features/persistence/services/persistence-service.cs:44-47
- Notes: Verified. Constructor does `new JsonSerializerOptions(timeWarpStateOptions.JsonSerializerOptions) { PropertyNameCaseInsensitive = true }` (clone only; shared Store/JsonRequestHandler options stay CamelCase and case-sensitive). Deserialize uses that field. Test `Load_Falls_Back_To_PascalCase_Name_Key` serializes with default options + `JsonStringEnumConverter` under `typeof(LocalWidgetState).Name` and asserts Guid/Kind. Readme notes load case-insensitivity.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:96-152
- Notes: Verified. `Handle` no longer serializes before the switch. Session/Local null checks call `LogMissingStorage` and break; only then `WriteAsync` receives `state` and calls `SerializeState`. `SerializeState` still uses `TimeWarpStateOptions.JsonSerializerOptions` (camelCase writes). Server/PreRender remain no-ops without serializing.

## Issues

No new issues found.
