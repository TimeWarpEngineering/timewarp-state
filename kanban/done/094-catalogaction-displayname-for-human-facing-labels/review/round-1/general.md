# Round 1 — general
**Date:** 2026-10-01
**Scope reviewed:** branch vs master (7e4608b1): attribute, entry, generator, analyzer, tests, docs, version bump

## Summary

Adds an optional authored `DisplayName` on `[CatalogAction]`, copied by the generator (via
`SymbolDisplay.FormatLiteral`, so quotes/escapes are safe) into a trailing optional `displayName`
constructor parameter on `ActionCatalogEntry`, emitted as a named argument (`null` when unset, no
fallback synthesized). TWS0008 rejects empty/whitespace; explicit `null` is treated as unset and
error-kind constants are skipped. Constancy is enforced by the compiler (CS0182), pinned by a test.
The binary-breaking constructor change is called out in the beta.7 release notes; version SSOT and
`source/Directory.Build.props` agree. Verified locally: `scripts/test.cs` all suites green
(analyzer 33, generator 15, client integration 56 + 1 skipped). Low risk.

## Issues

None.
