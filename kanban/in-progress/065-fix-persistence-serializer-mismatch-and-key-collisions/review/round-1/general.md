# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** branch `task/065-fix-persistence-serializer-mismatch-and-key-collis` vs `origin/master`

## Summary

The change correctly shares `TimeWarpStateOptions.JsonSerializerOptions` on save and load, writes via `SetItemAsStringAsync` under `Type.FullName`, falls back to `Name` on load, and caches enclosing type + `[PersistentState]` on each closed generic post-processor. Optional Blazored skip/warning, attribute aliasing, docs, round-trip tests, and leaving E2E ignored all match the brief. Dominant risk is migration: Name-key fallback finds leftover Blazored payloads, but default TimeWarp camelCase options do not bind Blazored 4.x’s default PascalCase JSON, so upgraded sessions can hydrate wiped state.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-plus/features/persistence/services/persistence-service.cs:51
- Description: Load tries `FullName` then `Name` as required, then deserializes with `TimeWarpStateOptions.JsonSerializerOptions` (`PropertyNamingPolicy = CamelCase`, `PropertyNameCaseInsensitive` false). Blazored.LocalStorage 4.5 / SessionStorage 2.4 default `SetItemAsync` uses plain `JsonSerializerOptions` (PascalCase property names). Verified with System.Text.Json: PascalCase JSON under CamelCase options binds as defaults (`Guid=empty`, `Count=0`) without throwing. `LoadPersistentStateRequest` then `SetState`s that non-null result, so Name-key leftovers are not preserved—they overwrite initialized state with empty values. Pre-change load used `new JsonSerializerOptions()`, which did bind those PascalCase Blazored payloads. `Load_Falls_Back_To_Simple_Name_Key` serializes the “legacy” payload with TimeWarp camelCase options, so it never exercises this shape.
- Suggestion: For persistence load (at least when reading the Name key), deserialize with case-insensitive options (clone TimeWarp options and set `PropertyNameCaseInsensitive = true`, or retry with that clone when the first bind looks empty). Add a round-trip/fallback test that seeds PascalCase JSON under `typeof(T).Name` and asserts Count/Guid survive. Do not rely on hosts remembering to flip global Store options.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:86
- Description: `SerializeState` runs before the switch confirms Session/Local storage is registered (and before Server/PreRender no-ops). That always pays serialize cost on the hot path, and if serialization throws while the storage service is null, the optional-Blazored “skip + warning” path never runs—whereas master skipped before `SetItemAsync` when storage was missing.
- Suggestion: Resolve `storageKey` early if desired, but call `SerializeState` only inside the SessionStorage/LocalStorage branches after the null-storage check (and only for those methods).
- Status: open
