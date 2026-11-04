# Decompose state-clone-planner.cs and cache symbol lookups
## Description

`source/timewarp-state-source-generator/state-clone-planner.cs` is a single 3416-line class. Split it into cohesive partial/separate types and remove the repeated symbol lookups that make every re-plan slow.

## Requirements

Decomposition (make the class `partial` first, then extract in this order):

1. `ClonePlanEmitter` (~640 lines): `MaterializeDispatches`, `Emit`, `ServiceTypes`, `IsPubliclyVisible`, `RememberICloneableState`, `NamedICloneableStates`, `CanNameFromGeneratedCode`, `UniqueName`, `StableHash`, `Sanitize`. Split `Emit` (164 lines) into header, extension methods, clone methods, initializers, holders.
2. `CloneDiagnostics` (~190): `Fail`, location helpers, `PropagateErrors`, `Report`, `HasDeeperMemberFailure`, `AnyReportedBelow`, `Reaches`, `RootSuffix`, `Mark`.
3. `SubtypeResolver` (~340): `FindImplementations`, `FindDerived`, `Subtypes`, `CloseOver`, `Unify`, `TryConstruct`, `SatisfiesConstraints`, `Substitute`, `CandidateTypes`, `TypesOf`, `IsFrameworkAssembly`, `Depth`.
4. `UnsafeAccessorHolders` (~270): `FieldAccessor`, `ConstructorAccessor`, `GetHolder`, `ConstraintClause`, `TypeText`, `Holder`.
5. `MetadataFieldInspector` (~330): `MetadataProblem`, `FindMetadataProblem`, `ReferenceAssemblyProblem`, `FullView`, member enumeration and attribute helpers.
6. `CollectionShapes` (~450): collection classifiers, case tables, `CollectionKind` matching. `CollectionKind` and `ConcreteCollections` to their own file.
7. `TypeCatalog` (~190): cached `GetTypeByMetadataName` lookups, `SymbolEquals`, `IsShared`, `IsComparer`, `AssignableTo`, `Named`.
8. Nested types `Slot`, `Holder`, `CollectionKind` to their own files.

Duplication to collapse while moving:
- `CollectClosedGenerateCloneTypes` and `CollectClosedICloneableStates` are the same scan with a different predicate.
- Field loop and property loop in `AppendExtraFields`.
- `TryAppendMember` / `TryAppendSharedMember`.
- `TryElementType` / `TryDictionaryArguments`.
- The two `Fallback` blocks (~1030, ~1096).
- `MatchInterface` chain of `SymbolEquals` -> table.

Dead code: `ICollection<T>`/`IList<T>` entries in `TryElementType` are unreachable after `IEnumerable<T>`; `IsTupleType` branch of `IsValueTuple`; `IPointerTypeSymbol` check at ~332; `CaseRequest.Depth` always 0 until rebuilt.

Performance:
- Resolve `GetTypeByMetadataName` once per compilation in `TypeCatalog`; `MatchConcrete` currently makes up to 24 calls per type and `IsShared` ~9 per slot.
- Cache `Subtypes` per target definition; `WrapForRuntimeType` makes it O(slots x types) twice. `CandidateTypes` rescans `ReferencedAssemblySymbols.Modules` on every call.
- Replace `ContainingNamespace.ToDisplayString()` comparisons in `ImplementsICloneable`, `Named`, `IsValueTuple` with symbol comparison.
- `RememberICloneableState` uses `List.Contains`; `Report` does `Roots.Any(Reaches)` which is O(R^2 x graph).

## Goal (implementer restatement)

Split the 3717-line `StateClonePlanner` into partials and separate types so each concern is readable, and stop resolving the same metadata names on every classification. Generated clone text and TWSG002 diagnostics stay the same: the existing generator tests are the check.

Acceptance:

- `StateClonePlanner` is `partial`. Emit, diagnostics, subtypes, accessors, metadata inspection, collections, and the type catalog each have their own file.
- `Slot`, `Holder`, `CollectionKind`, `SlotKind`, `CaseRequest`, and `CloneMember` are their own types.
- Repeated metadata-name lookups go through `StateCloneTypeCatalog`, built once per planner.
- Non-generic subtype lists and the "which referenced assemblies reference this one" scan are cached.
- Duplicate scans and dead branches called out in the brief are gone.
- `dotnet fixie timewarp-state-source-generator-tests` passes, which locks the generated shapes.

## Checklist

- [x] Class made partial, no behaviour change, tests green
- [x] Extractions 1-8 landed (one PR per two or three extractions is fine)
- [x] Duplication and dead code removed
- [x] TypeCatalog caching in place
- [x] Subtype cache in place
- [x] Generated output byte-identical for the existing shape tests before and after

## Depends on

- 134

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. The generator runs on `CompilationProvider` alone, so every keystroke replans; the caching items are what make that tolerable.

## Session

- Created: claude code review session (2026-11-04)
- Implementation: implementer oracle (2026-11-04)
- Boy scout: split the pre-existing files over the 500-line audit cap (analyzer, generator, state tests, and workflow-command). `scripts/e2e.cs` is a file-based app whose compile list does not include a sibling, so it was shortened to 500 lines in place. The named-element `IsTupleType` path stays: dropping it made `(string Name, Item It)` look like a hidden reference-assembly field and stopped emitting `ValueShapes.Clone`. `ganda repo audit --fix --checks kanban-landed` landed this kitchen on origin-home (`1dc7b6dc`). Re-audit has no errors. Advisory only: claim 098 has no worktree, and seven pre-existing paths are not kebab-case.

## Results

`StateClonePlanner` is a partial class. The old single file is the plan entry (`state-clone-planner.cs`, 78 lines). The rest is split by concern:

- `state-clone-planner.emit.cs` — dispatches, source text, service types, names
- `state-clone-planner.diagnostics.cs` — fail, propagate, report
- `state-clone-planner.subtypes.cs` — implementations, derived types, close-over
- `state-clone-planner.accessors.cs` — UnsafeAccessor holders
- `state-clone-planner.metadata.cs` — reference-assembly and hidden-field checks
- `state-clone-planner.collections.cs` and `state-clone-planner.collection-kinds.cs` — rebuild and kind matching
- `state-clone-type-catalog.cs` — one `GetTypeByMetadataName` per metadata name per compilation
- `state-clone-slot.cs`, `state-clone-holder.cs`, `state-clone-collection-kind.cs`, `state-clone-slot-kind.cs`, `state-clone-case-request.cs`, `state-clone-member.cs`

`StateCloneTypeCatalog` holds the BCL and TimeWarp symbols the planner used to look up on every type (`MatchConcrete` was up to 24 lookups, `IsShared` about 9). `ImplementsICloneable`, `IsValueTuple`, and the threading-namespace check compare symbols. Non-generic subtype lists are cached per target. Assemblies that reference a declaring assembly are cached so `CandidateTypes` does not rescan modules. `Report` walks each root once. The two closed-generic syntax scans share one walker. Field and auto-property copies share `AppendMember`. The two interface fallbacks share `MaterializeFallback`. `ICollection<T>` and `IList<T>` are no longer probed after `IEnumerable<T>`. The redundant pointer-symbol check is gone. `IsValueTuple` still accepts `IsTupleType` (a named tuple such as `(string Name, Item It)`); that path is the public element form, and the namespace check covers `System.ValueTuple`N` after `TupleUnderlyingType` is unwrapped. `RememberICloneableState` uses a set.

### How to validate

Smoke: `dotnet fixie timewarp-state-source-generator-tests`

Expect: 100 passed. The shape tests compile the generated clone and assert its text, so a byte change in the emitted source fails those cases. `dotnet fixie timewarp-state-analyzer-tests` passes the split handler cases (44). `dotnet build scripts/e2e.cs` and `dotnet build tools/dev-cli/dev.cs` succeed. `dotnet fixie timewarp-state-tests` runs the split clone-shape and transaction-behavior cases.
