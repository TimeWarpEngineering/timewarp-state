# Round 1 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 3 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/action-set-shape.cs:63 (action-set-method-generator.cs:93)
- Description: TWSG003, an Error, fires on record and struct Actions, and on ActionSets in record or struct states.
  The master generator ignored those shapes, so they compiled. Confirmed: a record Action in a non-partial state,
  and a record Action whose method name matches a state property, both fail the build now. A record parameter named
  `externalCancellationToken`, an abstract or private record, and a private parameter type on a record do too.
  Release notes and `analyzers.md` say every TWSG003 case used to be broken generated code. That is not true for
  these shapes.
- Suggestion: report TWSG003 only for shapes that were served before (a plain class Action in a class state). For a
  newly served shape, skip silently or downgrade to Warning or Info. Adjust the record-based TWSG003 tests and the
  wording in the docs and release notes.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/action-set-constructor-parser.cs:83 (test: action-set-method-generator-tests.cs:153)
- Description: A plain class Action with a primary constructor plus an explicit constructor used to get the explicit
  constructor's signature, for example `Set(string name)`. It now gets the primary one, `Set(string name, int count)`.
  Existing callers fail with CS7036 (confirmed), and the catalog parameters change. A test pins the breaking
  signature.
- Suggestion: for a plain class Action, prefer the first explicit constructor when one exists, and use the primary
  constructor only when there is no explicit one. Records and structs keep primary first. Flip the test.
- Source: general
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/action-catalog-source-generator.cs:88
- Description: `[CatalogAction]` on a record Action with a hand-written method of any other signature is now
  cataloged. The executor calls the user's method with generated-signature arguments plus
  `externalCancellationToken:`, which gives CS1739 or CS1501 in `TimeWarp.State.ActionCatalog.g.cs` (confirmed). On
  master, records were not cataloged, so this compiled.
- Suggestion: catalog a user-declared method only when its signature equals the generated one; otherwise skip the
  entry. Add a test.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. M1, M2 and M3 all violate the "Not breaking" requirement in different code paths, so they stay separate.
- Requirement tension behind M2: the task says the parser reads a primary constructor "before an explicit
  constructor" and also says "nothing that compiled changes meaning". The suggested fix meets both everywhere except
  the class-with-both-constructors overlap, where the non-breaking rule wins.
