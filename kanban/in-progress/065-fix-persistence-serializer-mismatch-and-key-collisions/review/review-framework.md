# Review framework — task 065

**Date:** 2026-09-21
**Host task:** kanban/in-progress/065-fix-persistence-serializer-mismatch-and-key-collisions/
**Diff scope:** branch `task/065-fix-persistence-serializer-mismatch-and-key-collis` vs `origin/master` (commit `f114a699`)
**Plan / brief:** Code review 2026-06-11 findings 5 and 18. Share `TimeWarpStateOptions.JsonSerializerOptions` on save and load; write storage keys as `Type.FullName`; load FullName then simple `Name`; cache enclosing state type + `[PersistentState]` on the closed generic post-processor; round-trip test with custom JSON / enum-as-string. Keep Blazored optional. Do not un-ignore E2E unless Playwright is green.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c317-2fe7-7432-adb9-4b074dab04e5` (2026-09-21); round 2 re-review of M1/M2 fix delta in the same session

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs`
- `source/timewarp-state-plus/features/persistence/services/persistence-service.cs`
- `source/timewarp-state-plus/features/persistence/persistent-state-storage-key.cs` (new)
- `source/timewarp-state/features/persistence/attributes/persistent-state-attribute.cs`
- `source/timewarp-state-plus/global-usings.cs`
- `source/timewarp-state-plus/readme.md`
- `tests/timewarp-state-plus-tests/features/persistence/persistence-round-trip-tests.cs` (new)
- `tests/timewarp-state-plus-tests/global-usings.cs`
- `tests/test-app-end-to-end-tests/persistence-test-page-tests.cs`

## Requirements to check

- Save and load use the same `TimeWarpStateOptions.JsonSerializerOptions`
- Save via string (`SetItemAsStringAsync`); load already `GetItemAsStringAsync`
- Storage key: write `FullName` (throw if null); load FullName then fall back to `Name`
- Document that new writes use FullName
- Cache enclosing state type + `[PersistentState]` as `static readonly` on each closed generic `PersistentStatePostProcessor<TRequest, TResponse>`
- Round-trip test: custom converter and/or enum-as-string
- Keep optional Blazored (skip + warning when storage not registered)
- Disambiguate `PersistentStateAttribute` vs ASP.NET’s
- E2E persistence stays ignored unless that path is actually green
