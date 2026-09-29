# Round 1 — general
**Date:** 2026-09-30
**Scope reviewed:** branch vs origin/master (57db7df7)

## Summary
The generator, analyzer and runtime types are largely sound: literals are escaped via SymbolDisplay.FormatLiteral, types are global::-qualified, the incremental model is equatable, and the 12 generator tests pass. The ActionSetMethodSourceGenerator extraction is behavior-preserving. Remaining concerns are executor argument conversion strictness, analyzer/generator acceptance mismatches, and a few test gaps.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state/features/action-catalog/action-catalog-arguments.cs:89
- Description: `Get<T>` only accepts `value is T`. No numeric widening (int boxed for a `long`/`decimal`/`double` parameter throws), and no conversion of the values the emitted InputSchema advertises (enum as string name, Guid/DateTime/DateOnly/TimeSpan as string, JsonElement). A host that builds arguments from the schema will hit ArgumentException.
- Suggestion: Either document that arguments must be pre-typed CLR values, or add AOT-safe conversions for primitives/enums/Guid/date types (Convert.ChangeType for IConvertible, Enum.Parse) and add a test for the mismatch case.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-analyzer/catalog-action-analyzer.cs:132
- Description: Design comment says the analyzer mirrors what the generator accepts, but the generator (action-catalog-source-generator.cs:58-59) also silently skips generic State/ActionSet/Action types and Actions not visible in the assembly (private/protected nesting). The analyzer reports nothing for these, so the attribute is silently omitted, the exact case TWS0004 is meant to prevent.
- Suggestion: Extend the placement check (or add a diagnostic) for generic containing types and inaccessible nesting.
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-state-source-generator/action-set-constructor-parser.cs:53
- Description: `p.Identifier.Text` keeps the verbatim `@` prefix (for example `@event`), so the catalog's parameter name literal and JSON schema property become "@event". Also the ActionSet method generator has the same text, so the C# call is fine, but the descriptor name is wrong.
- Suggestion: Use `Identifier.ValueText` for catalog names/schema (keep Text for emitted C# identifiers).
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state-analyzer/catalog-action-analyzer.cs:167
- Description: TWS0007 flags any ". " so abbreviations ("e.g. x", "Dr. Smith") produce false-positive warnings.
- Suggestion: Accept as known limitation or only flag terminator followed by space and an uppercase letter.
- Status: open

### Issue 5 — Severity: nit
- File: tests/client-integration-tests/features/action-catalog/action-catalog-tests.cs:7
- Description: No test for incremental caching (Run twice, assert cached/unchanged outputs), no test that ActionCatalog throws on cross-assembly duplicate names, and none for Get<T> type-mismatch or EnsureCount out-of-range behavior (requirements mention these behaviors).
- Suggestion: Add small unit tests for ActionCatalog duplicate throw, ActionCatalogArguments errors, and a generator caching test.
- Status: open
