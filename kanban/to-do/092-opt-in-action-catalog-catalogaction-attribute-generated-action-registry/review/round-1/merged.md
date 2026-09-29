# Round 1 — merged findings
**Date:** 2026-09-30
**Sources:** general, orchestrator (M6)

## Counts (final, after fix loop)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 2 | 2 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state/features/action-catalog/action-catalog-arguments.cs:28
- Description: `Get<T>` only accepted `value is T`; no numeric widening and no conversion of values the InputSchema advertises (enum names, Guid/date strings), so schema-driven hosts would get ArgumentException.
- Suggestion: AOT-safe conversions + test.
- Source: general
- Disposition notes: fixed — enum from name/integral, Guid/DateTime/DateTimeOffset/DateOnly/TimeOnly/TimeSpan from string, primitives/decimal/string via `Convert.ChangeType` (invariant culture); failures keep the ArgumentException with inner. Docs updated. Test `Execute_Should.Convert_String_And_Long_Arguments`.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-analyzer/catalog-action-analyzer.cs:132
- Description: Generator silently skips generic State/ActionSet/Action and inaccessible nesting; TWS0004 did not report them.
- Source: general
- Disposition notes: fixed — placement check now mirrors generator (`IsGenericType`, `IsVisibleInAssembly`); analyzer tests for generic state and private ActionSet.

### M3 — Severity: nit — Status: fixed
- File: source/timewarp-state-source-generator/action-set-constructor-parser.cs:53
- Description: Verbatim `@event` parameter leaked "@event" into catalog descriptor / schema names.
- Source: general
- Disposition notes: fixed — `ActionParameterModel.CatalogName` = `Identifier.ValueText` used by catalog; C# emission keeps `Identifier.Text`. Generator test added.

### M4 — Severity: nit — Status: wontfix
- File: source/timewarp-state-analyzer/catalog-action-analyzer.cs:167
- Description: TWS0007 flags any ". " so abbreviations ("e.g. x") warn.
- Source: general
- Disposition notes: wontfix (orchestrator) — TWS0007 is a warning-level heuristic; descriptions are meant to be one plain sentence for palette/agent display, where avoiding abbreviations is desirable. Suppressible per site.

### M5 — Severity: nit — Status: fixed
- File: tests/client-integration-tests/features/action-catalog/action-catalog-tests.cs:7
- Description: Test gaps — cross-assembly duplicate-name throw, Get/EnsureCount errors, generator caching.
- Source: general
- Disposition notes: fixed in part — arg count/type error tests already existed (`Reject_Wrong_Argument_Count/Type`); added `Catalog_Should.Throw_On_Duplicate_Names_Across_Sources`. Incremental-caching test not added: model equatability was verified by review (EquatableArray, record models) and no other generator in the repo has caching tests; not required by task.

### M6 — Severity: nit — Status: wontfix
- File: readme.md, tests/test-app/test-app-client/program.cs, source/timewarp-state-source-generator/action-set-method-generator.cs
- Description: CRLF→LF line-ending churn inflates the diff (whitespace-insensitive diff is small).
- Source: orchestrator
- Disposition notes: wontfix — no .gitattributes; LF normalization is harmless and the rest of the repo's new files are LF. Reviewers can use `git diff -w --ignore-cr-at-eol`.

## Duplicates / conflicts

- None.
