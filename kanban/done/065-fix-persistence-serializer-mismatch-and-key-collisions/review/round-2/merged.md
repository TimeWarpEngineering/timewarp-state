# Round 2 — merged findings
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
- File: source/timewarp-state-plus/features/persistence/services/persistence-service.cs:44
- Description: Load used shared CamelCase TimeWarp options, so leftover Blazored PascalCase JSON under Name hydrated as defaults.
- Suggestion: Clone TimeWarp options with `PropertyNameCaseInsensitive = true`; test PascalCase Name-key fallback.
- Source: general (round 1)
- Disposition notes: Round 2 verified the clone and `Load_Falls_Back_To_PascalCase_Name_Key`. Shared Store options are not mutated.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:142
- Description: Serialize ran before storage-null checks.
- Suggestion: Serialize only after Session/Local storage is present.
- Source: general (round 1)
- Disposition notes: Round 2 verified `WriteAsync` serializes after the null-storage check. Server/PreRender remain no-ops.

## Duplicates / conflicts

None. No new findings in round 2.
