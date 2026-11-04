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

## Checklist

- [ ] Class made partial, no behaviour change, tests green
- [ ] Extractions 1-8 landed (one PR per two or three extractions is fine)
- [ ] Duplication and dead code removed
- [ ] TypeCatalog caching in place
- [ ] Subtype cache in place
- [ ] Generated output byte-identical for the existing shape tests before and after

## Depends on

- 134

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. The generator runs on `CompilationProvider` alone, so every keystroke replans; the caching items are what make that tolerable.

## Session

- Created: claude code review session (2026-11-04)
