# Review framework — task 071

**Date:** 2026-09-21
**Host task:** kanban/in-progress/071-fix-source-generator-nested-class-and-culture-handling/
**Diff scope:** branch `task/071-fix-source-generator-nested-class-and-culture-hand` vs `origin/master` (implement commit `10bee665`; kanban results `102b953b`)
**Plan / brief:** Code review 2026-06-11 finding 15 (shrunk). Diagnostic (not nested partials) when `[PersistentState]` is on a nested class; skip emit. Hint names include containing types (or skip emit after diagnostic) so `AddSource` cannot collide. Delete unused `ToCamelCase`. Drop unused `ClassModel` fields if unused. Generator tests: nested class diagnostic; two same simple names in one namespace do not crash the generator. Optional: drop SG001 Unique Hint Name Info spam.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c347-06e4-7b72-ad20-40b78b8c1f4a` (2026-09-21); round 2 re-review of M1 fix delta in the same session

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state-source-generator/persistence-state-source-generator.cs`
- `source/timewarp-state-source-generator/AnalyzerReleases.Shipped.md` (new)
- `source/timewarp-state-source-generator/AnalyzerReleases.Unshipped.md` (new)
- `tests/timewarp-state-source-generator-tests/` (new Fixie project)
- `scripts/test.cs`
- `tools/dev-cli/endpoints/test-command.cs`
- `timewarp-state.slnx`
- `documentation/topics/analyzers.md`
- `source/timewarp-state-analyzer/readme.md`

## Requirements to check

- Nested `[PersistentState]` → diagnostic, no top-level `partial class {ClassName}` emit
- Hint names unique / no `AddSource` throw (containing types in hint, or skip emit after diagnostic)
- `ToCamelCase` deleted (do not “fix” culture); unused `ClassModel.PersistentStateMethod` dropped if unused
- Generator tests: nested diagnostic + no source; two same simple names in one namespace do not crash
- Optional: SG001 Unique Hint Name Info spam gone
- Out of scope: emitting a containing-type partial chain; persistence serializer/keys (065); PreRender/Server methods

## Round 2

Re-verify M1 against the post-fix delta. Also scan the fix for new defects. Do not clobber round 1.

Fix delta:

- `source/timewarp-state-source-generator/persistence-state-source-generator.cs` — `ClassModel` record + `EquatableDiagnosticLocation`; `Execute` uses `Location.Create`
- `source/timewarp-state-source-generator/is-external-init.cs` (new) — netstandard2.0 `IsExternalInit` polyfill for records
