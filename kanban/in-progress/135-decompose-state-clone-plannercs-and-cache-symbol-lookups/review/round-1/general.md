# Round 1 — general
**Date:** 2026-11-04
**Scope reviewed:** branch `task/135-decompose-state-clone-plannercs-and-cache-symbol-l` vs merge-base `5d0564eb`, excluding `kanban/`. That covers the planner split, the new `state-clone-*.cs` types and `StateCloneTypeCatalog`, and the boy-scout splits of tests, `scripts/e2e.cs` and `tools/dev-cli/endpoints/workflow-command*.cs`.

Effort 3 used three `general` passes, split by scope:
- (a) catalog, symbols, subtypes, roots and collections
- (b) emit, diagnostics, metadata, accessors, construction, objects, shapes, slots and the small types
- (c) the non-generator boy-scout splits

The orchestrator re-checked every finding below against the code.

## Summary

The split is mostly a faithful move. Pass (b) matched every chunk of `accessors.cs` and `construction.cs` verbatim against the old file. `Emit` text, diagnostic messages, locations and dedupe keys are the same. The member and fallback merges keep every branch. Moved types are still reference-equality classes. No test was lost in the boy-scout splits, and the line content is the same before and after.

One real behavior change slipped in. Moving the namespace-string checks to `INamespaceSymbol` equality breaks the `System.Threading` / `System.Threading.Tasks` "shared" rule in any build against the reference pack. The generator tests can't see it because their test driver compiles against `TRUSTED_PLATFORM_ASSEMBLIES` (implementation assemblies, where everything sits in `System.Private.CoreLib`).

The brief also asked for `Emit` to be split, and it was not.

Gates run:
- `dotnet fixie timewarp-state-source-generator-tests`: 100 passed.
- Pass (c) ran the analyzer tests (44/44), the state tests (131 passed, 1 skipped), and `dotnet build tools/dev-cli/dev.cs`, which succeeded.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-source-generator/state-clone-type-catalog.cs:25-26, :154-156 (used from state-clone-planner.symbols.cs:264)
- Description:
  - Old `IsShared` treated any type in a namespace whose display string is `System.Threading` or `System.Threading.Tasks` as shared, whatever assembly declared it.
  - New `InThreadingNamespace` compares `type.ContainingNamespace` with `SymbolEqualityComparer.Default` against the namespace of `System.Threading.Thread` and the namespace of `System.Threading.Tasks.Task`.
  - Metadata namespace symbols belong to one module. They are only equal when both types come from the same assembly.
  - Verified with a Roslyn 4.14 probe against `Microsoft.NETCore.App.Ref/10.0.9/ref/net10.0`:
    - `Thread` is in `System.Threading.Thread`.
    - `CancellationTokenSource` and `Lock` are in `System.Runtime`, and `sameNsAsThread=False`.
    - `SemaphoreSlim` is in `System.Threading`, and `sameNsAsThread=False`.
    - `Parallel` is in `System.Threading.Tasks.Parallel`, and `sameNsAsTask=False`.
  - Effect: in a consumer build, a state member typed `CancellationTokenSource`, `SemaphoreSlim`, `Lock`, `Timer`, `ManualResetEventSlim` and so on is no longer copied by reference. It falls through to object classification. That yields a new TWSG002 (reference assembly hides private fields) or a field-wise clone of an identity-bound object. This contradicts the restated acceptance ("Generated clone text and TWSG002 diagnostics stay the same"). The checklist claims byte-identical output, but that claim was only proven against TPA compilations.
- Suggestion:
  - Compare the namespace by structure, which does not allocate: `ns is { Name: "Threading", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } }`, plus the `Tasks` level. Or restore the `ToDisplayString()` comparison.
  - Add a generator test that compiles against reference-pack assemblies (or a `ReferenceAssemblies` set), with a state member typed `CancellationTokenSource` and one typed `SemaphoreSlim`, and asserts reference copy with no TWSG002.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-source-generator/state-clone-type-catalog.cs:23, :120-152; state-clone-planner.metadata.cs:43-45; state-clone-planner.symbols.cs:247-253
- Description:
  - Several other name or namespace string checks became exact symbol identity against `GetTypeByMetadataName` results:
    - `ImplementsICloneable` (old: `Name == "ICloneable"` and namespace `"System"`).
    - The `ReferenceAssemblyAttribute` check (old: `AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.ReferenceAssemblyAttribute"`).
    - The `IsShared` named-type list (old: `Named(type, ns, name)` strings).
    - The `IsValueTuple` fallback, which compares against the namespace of `System.Object`. That is the same module-scoped flaw as Issue 1.
  - `GetTypeByMetadataName` returns null when a name is ambiguous across references, and it prefers a type declared in the compilation itself (for example an internal polyfill attribute). In those cases the new checks silently stop matching.
  - The worst case is the reference-assembly check. A type from a reference assembly would then skip TWSG002 and get a shallow clone.
  - None of these differ for plain net10 references, so this is an edge-case risk, not an observed regression.
- Suggestion: for these predicates, use a structural name and namespace check. It is cheap and needs no lookup. Or fall back to the name check when the catalog symbol is null. The fix for Issue 1 can share the same helper.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-source-generator/state-clone-planner.emit.cs:126
- Description:
  - Requirement 1 says: "Split `Emit` (164 lines) into header, extension methods, clone methods, initializers, holders."
  - `Emit` was moved word for word. It is still one method, about 170 lines (126-296).
  - The implementer restatement dropped this sub-requirement, and the Results do not mention it.
- Suggestion: extract `EmitHeader`, `EmitExtensionMethods`, `EmitCloneMethods`, `EmitInitializers` and `EmitHolders`, each appending to the shared builder. The shape tests already lock the byte output.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-state-source-generator/state-clone-planner.diagnostics.cs:82-86
- Description:
  - `Report` runs on every plan (state-clone-planner.cs:73). It now builds a full `Reachable(root)` set for every root before checking whether any slot failed.
  - The old `Reaches` ran lazily, only for roots in error. So the happy path, which is every keystroke with no errors, now pays O(roots × graph) time and memory.
  - That is the opposite of this task's performance goal. The result is equivalent; only the cost changed.
- Suggestion: build `reachableFromRoot` lazily, only for roots in `reportedRoots`, and only after the first loop has found at least one reported root. Or return early when `reportedRoots` is empty.
- Status: open

### Issue 5 — Severity: bug
- File: scripts/e2e.cs:12, ~:156, ~:459, ~:474
- Description:
  - To get exactly under the 500-line audit cap (the `ganda repo audit` file-length check), the file was cut from 517 to 500 lines without any decomposition. The cuts:
    - Two "why" comments that carry information:
      - "Wait on both paths: the OutputDataReceived handlers may still be draining…"
      - "Best effort: a partially trusted cert must not fail the run."
    - Detail from the Design region (port 7011, 30 retries, `runMode` fixed to Auto, the `NUGET_PACKAGES` isolation, dev-certs skipped over http). It is now one 185-character line.
    - The blank line between `Main` and `WriteStepHeader`.
  - The only effect of these cuts is to make the line-cap audit pass without the work that audit measures. Per the review rules, that is a bug.
  - No logic changed. Pass (c) diffed it.
- Suggestion: restore the comments and the wrapped Design text. Then decompose the script for real: a file-based app can pull shared code through a `#:project` reference or a small helper project. If decomposition is out of scope for task 135, revert `scripts/e2e.cs` to its pre-task content and record the over-cap file as a follow-up instead of squeezing it.
- Status: open

### Issue 6 — Severity: nit
- File:
  - source/timewarp-state-source-generator/state-clone-type-catalog.cs (`ValueTuple`, `Named`)
  - tests/timewarp-state-tests/cloning/generated-clone-shape-more-tests.cs:6
  - tests/timewarp-state-source-generator-tests/state-clone-shape-tests.cs:9
  - tests/timewarp-state-analyzer-tests/handler-must-not-send-action-analyzer-tests.cs (Purpose)
  - tests/timewarp-state-source-generator-tests/state-clone-correctness-record-tests.cs:82
- Description: small leftovers and stale text:
  - `Catalog.ValueTuple` and `Catalog.Named` have no callers, and `Named` is a copy of `SymbolEquals`.
  - The old `SymbolEquals` guarded against an empty metadata name; the new `Resolve("")` does not.
  - `generated-clone-shape-more-tests.cs:6` says the fixtures stay in the first file "so the generator sees them". The generator sees the whole compilation.
  - `state-clone-shape-tests.cs:9` still names only `generated-clone-shape-tests.cs` for runtime coverage.
  - The analyzer test Purpose still lists the negative cases, which moved to the `-negative-tests.cs` file.
  - `CorrectnessDiagnostics` now lives in the smaller record file, away from most of its callers.
- Suggestion: delete the unused members, restore the empty-name guard, fix the three comments, and move `CorrectnessDiagnostics` next to its main callers.
- Status: open

## Checked and found equivalent

- **Catalog metadata names:** every name and arity suffix matches the old literals, and the tables keep their order.
- **Null handling** is preserved.
- **`TryElementType`:** dropping `ICollection<T>`/`IList<T>` is safe, because the `IEnumerable<T>` check on the type and on `AllInterfaces` comes first.
- **`IPointerTypeSymbol` removal** is safe, because `TypeKind.Pointer` already covers it.
- **Subtype cache:** keyed on non-generic targets with `SymbolEqualityComparer.Default`. Generic targets bypass it. Callers never mutate the returned list.
- **Referencing-assemblies cache:** depends only on the compilation, and keeps the `ReferencedAssemblySymbols` order.
- **`CollectClosedGenericNames`:** the shared walker keeps the predicates and the order of the old two methods.
- **`RememberICloneableState`:** the list is kept, and the set only dedupes. Emitted order is still sorted ordinally.
- **`AppendMember` and `MaterializeFallback`:** they produce identical lines and the same early returns.
- **`Report`:** the single walk per root reports the same set of diagnostics, in the same order.
- **Boy-scout test splits:** all test names are present after the split, with unchanged bodies. The `workflow-command.release.cs` partial is compiled through `Directory.Build.props` `endpoints/**/*.cs`.
