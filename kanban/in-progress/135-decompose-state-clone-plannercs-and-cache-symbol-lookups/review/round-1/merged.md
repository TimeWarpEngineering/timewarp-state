# Round 1 — merged findings
**Date:** 2026-11-04
**Sources:** general (three scope passes a/b/c, merged into `general.md`)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-state-source-generator/state-clone-type-catalog.cs:25-26, :154-156
- Description:
  - `InThreadingNamespace` compares module-scoped `INamespaceSymbol`s. Against the net10 reference pack it no longer matches `CancellationTokenSource`, `SemaphoreSlim`, `Lock`, `Parallel` and similar types, because they are declared in other assemblies than `Thread` and `Task`.
  - Those members lose the "shared" (reference-copy) rule and get a new TWSG002 or a field-wise clone.
  - Verified with a Roslyn probe against the ref pack. The tests miss it because the driver compiles against TPA implementation assemblies.
- Suggestion: use a structural namespace check, or restore `ToDisplayString()`. Add a ref-pack generator test with `CancellationTokenSource` and `SemaphoreSlim` members.
- Source: general (a), orchestrator-verified
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: scripts/e2e.cs:12, ~:156, ~:459, ~:474
- Description: 517 → 500 lines, done by deleting "why" comments, collapsing the Design region and removing a separator line. The only effect is to pass the `ganda repo audit` 500-line file cap without decomposing anything. Review rules make that a bug.
- Suggestion: restore the comments and the Design text. Then decompose the script for real (`#:project` or a helper), or revert to the pre-task file and record the over-cap file as a follow-up.
- Source: general (c), reclassified by orchestrator under the audit-gaming rule
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-state-source-generator/state-clone-type-catalog.cs:23, :120-152; state-clone-planner.metadata.cs:43-45; state-clone-planner.symbols.cs:247-253
- Description: `ImplementsICloneable`, the `ReferenceAssemblyAttribute` check, the `IsShared` named-type list and the `IsValueTuple` fallback switched from name strings to symbol identity against `GetTypeByMetadataName`. They silently stop matching when that lookup returns null (ambiguous name) or returns a polyfill copy declared in the compilation. The `IsValueTuple` fallback has the same module-scoped namespace flaw as M1.
- Suggestion: use structural name and namespace checks, or fall back to names when the catalog symbol is null. Share the helper with M1.
- Source: general (a), general (b)
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.emit.cs:126
- Description: Requirement 1 says to split `Emit` into header, extension methods, clone methods, initializers and holders. It was moved verbatim and is still one method of about 170 lines. The restatement dropped this sub-requirement.
- Suggestion: extract the five emit helpers. The shape tests lock the bytes.
- Source: general (b), orchestrator against the task brief
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-state-source-generator/state-clone-planner.diagnostics.cs:82-86
- Description: `Report` runs on every plan. It now builds `Reachable(root)` for every root up front, where the old code checked reachability lazily and only for roots in error. That adds O(roots × graph) to the no-error path, which works against the task's replan-performance goal.
- Suggestion: compute reachability lazily, only for roots in `reportedRoots`. Return early when that set is empty.
- Source: general (b)
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: state-clone-type-catalog.cs; generated-clone-shape-more-tests.cs:6; state-clone-shape-tests.cs:9; handler-must-not-send-action-analyzer-tests.cs (Purpose); state-clone-correctness-record-tests.cs:82
- Description:
  - `Catalog.ValueTuple` and `Catalog.Named` are unused (and `Named` duplicates `SymbolEquals`).
  - The empty-metadata-name guard was dropped.
  - Three comments are stale after the splits.
  - `CorrectnessDiagnostics` sits away from its main callers.
- Suggestion: delete the unused members, restore the guard, fix the comments, and move the helper.
- Source: general (a), general (c)
- Disposition notes:

## Duplicates / conflicts

- The `IsValueTuple` namespace concern was raised by both (a) and (b), and the symbol-identity concern by both. They are collapsed into M3.
- (c) raised e2e comment deletion as a suggestion. The orchestrator raised it to bug (M2) under the rule that a change whose only effect is passing an audit is a bug. The cap gamed is the `ganda repo audit` 500-line file check.
- (c)'s stale-comment and placement nits and (a)'s unused-member nits are collapsed into M6.
