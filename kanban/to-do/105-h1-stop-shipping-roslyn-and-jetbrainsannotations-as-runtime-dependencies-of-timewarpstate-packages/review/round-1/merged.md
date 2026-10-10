# Round 1 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: nit — Status: fixed
- File: tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs:73
- Description: The `Directory.Build.props` Roslyn guard matched two literal strings, so other PackageReference forms got past it.
- Suggestion: Parse the XML and check the PackageReference Include values.
- Source: general
- Disposition notes: Fixed. The test now parses `Directory.Build.props` with XDocument and asserts that no `PackageReference` Include equals `Microsoft.CodeAnalysis.CSharp`. `dotnet test tests/timewarp-state-tests`: 101 passed, 1 skipped.

### M2 — Severity: nit — Status: fixed
- File: task.md Results (How to validate)
- Description: The Results called `./bin/dev` a checked-in binary. git does not track it.
- Suggestion: Reword it.
- Source: general
- Disposition notes: Fixed. It now reads "locally built `./bin/dev` binary (not tracked by git)".

## Duplicates / conflicts

- None (single reviewer).
