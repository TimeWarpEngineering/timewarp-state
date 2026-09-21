# Round 1 — merged findings
**Date:** 2026-09-21
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-state-plus/features/persistence/services/persistence-service.cs:51
- Description: Load tries `FullName` then `Name` as required, then deserializes with `TimeWarpStateOptions.JsonSerializerOptions` (`PropertyNamingPolicy = CamelCase`, `PropertyNameCaseInsensitive` false). Blazored.LocalStorage 4.5 / SessionStorage 2.4 default `SetItemAsync` uses plain `JsonSerializerOptions` (PascalCase property names). PascalCase JSON under CamelCase options binds as defaults (`Guid=empty`, enum/count zero) without throwing. `LoadPersistentStateRequest` then `SetState`s that non-null result, so Name-key leftovers overwrite initialized state instead of preserving it. Pre-change load used `new JsonSerializerOptions()`, which did bind those PascalCase Blazored payloads. `Load_Falls_Back_To_Simple_Name_Key` serializes the “legacy” payload with TimeWarp camelCase options, so it never exercises this shape.
- Suggestion: For persistence load, deserialize with a clone of TimeWarp options that sets `PropertyNameCaseInsensitive = true` (do not mutate the shared Store/JsonRequestHandler instance). Add a fallback test that seeds PascalCase JSON under `typeof(T).Name` and asserts Guid/Kind survive.
- Source: general
- Disposition notes: Load clones TimeWarp options with `PropertyNameCaseInsensitive = true`. Test `Load_Falls_Back_To_PascalCase_Name_Key` seeds PascalCase JSON under Name. Shared Store options are not mutated.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:86
- Description: `SerializeState` runs before the switch confirms Session/Local storage is registered (and before Server/PreRender no-ops). That always pays serialize cost on the hot path, and if serialization throws while the storage service is null, the optional-Blazored “skip + warning” path never runs—whereas master skipped before `SetItemAsync` when storage was missing.
- Suggestion: Call `SerializeState` only inside the SessionStorage/LocalStorage branches after the null-storage check.
- Source: general
- Disposition notes: `WriteAsync` serializes after the Session/Local null-storage check. Server/PreRender remain no-ops.

## Duplicates / conflicts

None — single reviewer, two distinct issues.
