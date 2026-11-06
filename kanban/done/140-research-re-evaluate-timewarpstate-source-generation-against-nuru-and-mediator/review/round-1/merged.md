# Round 1 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: report/analysis.md:208, with the §4.1 catalog row at `:59`
- Description: Option 0 says `[CatalogAction]` on a record action "starts producing executors". The catalog generator's predicate is `node is ClassDeclarationSyntax` (`source/timewarp-state-source-generator/action-catalog-source-generator.cs:33`), and the shared constructor parser takes a `ClassDeclarationSyntax` (`action-set-constructor-parser.cs:27-38`). Records stay invisible to the catalog unless Option 0 also changes the catalog generator, and Option 0 does not list that change.
- Suggestion:
  - Add the catalog predicate and parameter-reader change to Option 0's "What changes".
  - Note the record gap in the §4.1 catalog row.
  - Optionally note that the parser uses the first descendant constructor.
- Source: general
- Disposition notes: Fixed 2026-11-06 (implement-fix-2). Option 0 "What changes" now has a catalog-generator bullet: widen the predicate to `TypeDeclarationSyntax`, and change `ActionSetConstructorParser.GetParameters` to take a `TypeDeclarationSyntax` and read the primary-constructor or positional parameter list before the first explicit constructor. The Option 0 cost line points at that change. The §4.1 catalog row now names the `ClassDeclarationSyntax` predicate (`:33`), the parser (`:27-38`), its first-descendant-constructor rule, and the record gap.

### M2 — Severity: nit — Status: fixed
- File: report/analysis.md:41, :93, :99, :105, :106, :423
- Description: Off citations and counts:
  - `MG/candidate-facts.cs` should be 19-65.
  - `MG/mediator-model.cs:208-275` should be 21-89 (the file has 89 lines).
  - F9 should be 287-301.
  - `MG/` has 16 files, not 17.
  - `SG/` has 29 files, not 31.
- Suggestion: correct them.
- Source: general
- Disposition notes: Fixed 2026-11-06 (implement-fix-2). Corrected to `MG/candidate-facts.cs:19-65`, `MG/mediator-model.cs:21-89`, F9 `:287-301` (in §4.2 and §6), `MG/` 16 files, `SG/` 29 files. Each was re-checked against the source.

### M3 — Severity: nit — Status: fixed
- File: report/analysis.md:27
- Description: The executive summary says any nested `*ActionSet` "in any class" produces broken code. The broken case is only a class that is not a partial state.
- Suggestion: reword to match §4.1 and task.md Results ("outside a state").
- Source: general
- Disposition notes: Fixed 2026-11-06 (implement-fix-2). The executive summary now says a nested `*ActionSet` inside a class that is not a partial state produces broken code, which matches §4.1 and the task Results.

## Duplicates / conflicts

- None. There was a single reviewer.
