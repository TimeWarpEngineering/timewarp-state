# Round 1 — merged findings
**Date:** 2026-11-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: nit — Status: open
- File: .editorconfig:297
- Description: "2.0.0-beta.29 added TWA0025 …" lost its package name when the header was rewritten to name only TimeWarp.SourceGenerators 1.0.0-beta.13, so it reads as a SourceGenerators version. The block also does not say that the Architecture-app `none` ids are no longer reported by any referenced package.
- Suggestion: Write "TimeWarp.Architecture.Analyzers 2.0.0-beta.29 added …". Add one clause that the Architecture-app ids stay `none` but are inert now that State no longer references Architecture.Analyzers (task 153). Keep the severity lines (requirement 4). Optionally, correct the fixie cause sentence in `## Results` to "stale `~/.dotnet/toolResolverCache`" (see general.md Observations).
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
