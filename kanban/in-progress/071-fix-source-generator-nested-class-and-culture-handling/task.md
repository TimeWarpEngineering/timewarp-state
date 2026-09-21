# Persistence generator: reject nested states; unique hint names; delete dead ToCamelCase

## Description

Code review 2026-06-11, finding 15 — **shrunk**. 075/080 changed emit to `Load()` + `LoadPersistentStateRequest`; nesting and hint names are **unchanged**.

`GetSemanticTarget` still records only namespace + class identifier. Nested `[PersistentState]` still emits a **top-level** `partial class {ClassName}`. Two same-named nested types still collide on `{Namespace}.{ClassName}_Persistence.g.cs`. Policies require **actions** nested in states, not nested states — nested `[PersistentState]` is **unsupported**.

`ToCamelCase` (`char.ToLower`) is **dead** (nothing calls it). Do not “fix” it — **delete** it. `PersistentStateMethod` on `ClassModel` is unused in `GenerateLoadClassCode` (runtime reads the attribute); drop it if it stays unused.

## Depends on

065

## Requirements

- Diagnostic (not nested partials) when `[PersistentState]` is on a nested class; skip emit.
- Hint name includes containing types (or skip emit after diagnostic) so `AddSource` cannot collide.
- Delete unused `ToCamelCase`. Drop unused `ClassModel` fields if unused.
- Generator tests: nested class diagnostic; two same simple names in one namespace do not crash the generator.
- Optional: drop `SG001` “Unique Hint Name” Info spam if still present.

## Out of scope

- Emitting a full containing-type chain (not a product feature)
- Persistence serializer/keys (065)
- PreRender/Server methods

## Checklist

- [x] Nested `[PersistentState]` → diagnostic, no bad partial
- [x] Hint names unique / no AddSource throw
- [x] `ToCamelCase` gone
- [x] Generator tests as above
- [x] Review round 1 (general, effort 1)
- [x] Fix M1 on this task id
- [x] Review round 2 re-verify; disposition clean

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk; Depends on 065. Dispatch after 065 merges.
- Implementer: grok (2026-09-21) — TWSG001 skip-emit, unique hints, drop ToCamelCase/SG001.
- Review: grok oracle `01a0c347-06e4-7b72-ad20-40b78b8c1f4a` (2026-09-21) — effort 1 general; rounds 1–2; disposition clean.

## Results

Nested `[PersistentState]` no longer emits a top-level `partial class {ClassName}`. The persistence generator reports **TWSG001** (Error, Persistence) on the nested type identifier and skips `AddSource`. Hint names include containing types with CLR `+` (`App.Outer+WidgetState_Persistence.g.cs`) so two same simple names cannot collide even if emit is added later. Dead `ToCamelCase`, unused `ClassModel.PersistentStateMethod` / `GetPersistentStateMethod`, and SG001 “Unique Hint Name” Info spam are gone. `Load()` still sends `LoadPersistentStateRequest`; the attribute is read at runtime.

**Files**

- `source/timewarp-state-source-generator/persistence-state-source-generator.cs`
- `source/timewarp-state-source-generator/is-external-init.cs`
- `source/timewarp-state-source-generator/AnalyzerReleases.Shipped.md`
- `source/timewarp-state-source-generator/AnalyzerReleases.Unshipped.md`
- `tests/timewarp-state-source-generator-tests/` (new Fixie project)
- `scripts/test.cs`, `tools/dev-cli/endpoints/test-command.cs`, `timewarp-state.slnx`
- `documentation/topics/analyzers.md`, `source/timewarp-state-analyzer/readme.md`

**Decisions**

- Diagnostic + skip emit, not a containing-type partial chain (out of scope).
- Id `TWSG001` so it does not collide with analyzer `TWS0001`–`TWS0003` / `TWS001` or TimeWarp.SourceGenerators `TW0001`–`TW0006`.
- Error severity: nested persistence is unsupported; the old emit already failed to compile.
- Incremental model stores equatable path+span, not Roslyn `Location` (review M1).

**Review**

- Effort 1; roster: general; rounds: 2
- Final counts: bug 0/0/0 open/fixed/wontfix; suggestion 0 open / 1 fixed / 0 wontfix; nit 0
- **Disposition: clean** (M1 suggestion fixed on this task id; round 2 found no new issues)
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/merged.md`, `review/disposition.md`

**Tests:** `dotnet fixie timewarp-state-source-generator-tests` — 4 passed (nested TWSG001 + no source; two nested `WidgetState` in one namespace do not throw; top-level still emits `Load()`; top-level + nested same simple name does not throw). `dotnet build tests/test-app/test-app-client/test-app-client.csproj` — 0 errors (existing top-level Purple/Blue `[PersistentState]` still generate).

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet build ./tests/timewarp-state-source-generator-tests/timewarp-state-source-generator-tests.csproj
dotnet fixie timewarp-state-source-generator-tests
```

**Expect:** build 0 errors / 0 warnings; Fixie **4 passed**. Nested `[PersistentState]` reports `TWSG001` and produces no `*_Persistence.g.cs`. Two nested types with the same simple name in one namespace do not throw `ArgumentException` from `AddSource`. Top-level `[PersistentState]` still emits `{Namespace}.{ClassName}_Persistence.g.cs` containing `LoadPersistentStateRequest`.

**Automated gate**

```bash
dotnet fixie timewarp-state-source-generator-tests
# expect: 4 passed
```

`./bin/dev test` / `scripts/test.cs` now include this suite after analyzer tests.

**Not in scope:** emitting a containing-type partial chain; persistence serializer/keys (065); PreRender/Server methods.
