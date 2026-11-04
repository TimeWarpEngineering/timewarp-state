# Round 2 — general
**Date:** 2026-11-04
**Scope reviewed:** fix delta `1ac6afb4..8eadac32` (commit `8eadac32`, "fix: match clone namespaces by structure and split the e2e runfile"), excluding `kanban/`. Also re-checked round-1 M1–M6 against the post-fix tree.

## Summary

The fix pass covers all six round-1 findings. There are now four structural name checks: `HasMetadataName` and `InNamespace` walk the name and each containing namespace, and `IsValueTuple` and `InThreadingNamespace` call `InNamespace`. These checks replace symbol identity for threading, tasks, `ICloneable`, `ReferenceAssemblyAttribute`, the shared BCL names, and the `ValueTuple` fallback. A new test compiles against the reference pack and shows the module split that hid M1. `Report` now computes reachability lazily. `Emit` is five writers. The e2e runfile is a pure move into `#:include` partials. I found no new defects in the delta.

## Re-verification of round-1 findings

- **M1 (bug) — fixed.** `state-clone-type-catalog.cs:169-171` matches `System.Threading` and `System.Threading.Tasks` with `InNamespace`, a structural walk that stops at the global namespace. I traced the segment walk for one-, two- and three-segment names; it is correct. `state-clone-reference-pack-tests.cs` asserts `Thread.ContainingNamespace != CancellationTokenSource.ContainingNamespace` on the net11.0 ref pack. That is the exact condition the round-1 probe found. It also expects reference copies and no TWSG002, and it passes.
- **M2 (bug) — fixed.** I compared the comment and code lines of the pre-task `scripts/e2e.cs` (`fab5a9fc~1`, 517 lines) with the union of `scripts/e2e*.cs`:
  - Every original comment line is present.
  - The only code lines that differ are `static class App` → `static partial class App`, the four `#:include` directives, and the new Purpose/Design regions.
  - It is a pure move, and nothing was deleted to clear the line cap. `dotnet build scripts/e2e.cs` succeeds.
- **M3 (suggestion) — fixed.** `ImplementsICloneable`, `HasReferenceAssemblyAttribute`, `IsSharedNamedType`, and the `IsValueTuple` fallback no longer depend on `GetTypeByMetadataName`. These checks now behave as the pre-task `Named(type, ns, name)` and `ToDisplayString()` checks did, without allocating display strings.
- **M4 (suggestion) — fixed.** `Emit` calls `EmitHeader`, `EmitExtensionMethods`, `EmitCloneMethods`, `EmitInitializers`, and `EmitHolders`. The shape tests still lock the emitted text (generator tests 101/101).
- **M5 (suggestion) — fixed.** The eager `Reachable(root)` loop is gone. `ReachedByReportedRoot` computes and caches `Reachable(reported)` only for roots that already produced a diagnostic. `AnyReportedBelow` returns early when no root did. `ReportUncoveredRoots` does not add to `reportedRoots`, so lazy caching gives the same diagnostics as the eager map.
- **M6 (nit) — fixed.**
  - `Catalog.ValueTuple`, `Named`, and the cached namespace symbols are removed.
  - `Resolve` and `SymbolEquals` guard an empty metadata name.
  - The three stale comments now name files that exist (`handler-must-not-send-action-analyzer-negative-tests.cs` exists).
  - `CorrectnessDiagnostics` moved to `state-clone-correctness-tests.cs`.

## Issues

None.

## Observations (not findings)

- Requirement "replace `ToDisplayString()` comparisons … with symbol comparison" is deliberately not met literally. Namespace symbol comparison is the M1 bug. The structural walk keeps the requirement's intent: no display-string allocation per check. The restatement records this explicitly, so it is not a misread.
- `Report` has two branches that differ only in whether they pass `null` or `[]` for `reachableFromRoot`. When `reportedRoots` is empty, `ReachedByReportedRoot` would return false anyway. The branch is redundant but harmless.
- `ReferencePackReferences` hard-codes `net11.0`, the same as `Directory.Build.props` and the Playwright path in `e2e-build.cs`. When the TFM is bumped, it fails loudly with a clear message.
- `HasMetadataName` does not exclude nested types (a nested `Guid` in a type in namespace `System` would match). The pre-task `Named` check behaved the same way, so this is not a regression.

## Gates run by this review

- `dotnet fixie timewarp-state-source-generator-tests`: 101 passed, including `Should_Share_Threading_Types_From_The_Reference_Pack`.
- `dotnet fixie timewarp-state-analyzer-tests`: 44 passed.
- `dotnet fixie timewarp-state-tests`: 131 passed, 1 skipped.
- `dotnet build scripts/e2e.cs`: succeeded.
- `ganda repo audit`: passed. It gave two pre-existing advisory warnings, `kanban-landed` (098) and `kebab-path-names` (7 pre-existing paths). Neither is a path this task changed.
