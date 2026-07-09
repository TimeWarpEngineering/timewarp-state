# Fix analyzer State type matching and remove debug scaffolding

## Description

Code review 2026-06-11, findings 14 and 26 (`code-review-2026-06-11.md`).

**Simple-name type matching (finding 14):** all three analyzers in `source/timewarp-state-analyzer/` identify TimeWarp state types by the base type's *simple* name:

- `state-implementation-analyzer.cs:44` — `symbol.BaseType.Name == "State" && symbol.BaseType.TypeArguments.Length == 1`
- `state-inheritance-analyzer.cs:39` — `!baseTypeSymbol.Name.Equals("State")`
- `state-read-only-public-properties-analyzer.cs:53` — same check, walking the entire base chain

Any consumer deriving from a *different* library's `State<T>` base gets error-severity `TWS001` false positives that break their build. Fix: resolve `` TimeWarp.State.State`1 `` once via `compilation.GetTypeByMetadataName` (in a compilation-start action) and compare with `SymbolEqualityComparer` against the original definition.

**Dead debug scaffolding (finding 26):** `timewarp-state-action-analyzer.cs` ships `DebugRule`/`TWD001` in `SupportedDiagnostics` (line 41) whose only producer, `ReportDebugInformation` (lines 60–64), has zero call sites — a registered diagnostic ID that can never fire. `LaunchDebugger`'s only call site is commented out (line 45); `Debugger.Launch` code in a shipped NuGet analyzer is one accidental uncomment away from freezing every consumer's build. Delete `DebugRule`, `DebugDiagnosticId`, `ReportDebugInformation`, and `LaunchDebugger`.

## Checklist

- [ ] Metadata-name matching in all three analyzers (shared helper)
- [ ] Analyzer test: a foreign `State<T>` base class produces no diagnostics
- [ ] Existing analyzer tests still pass for real TimeWarp states
- [ ] Delete TWD001 scaffolding and LaunchDebugger
