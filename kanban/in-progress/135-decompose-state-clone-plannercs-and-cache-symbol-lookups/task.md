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

`StateClonePlanner` was one class that resolved the same metadata names on every type. Split it by concern, cache those lookups, and keep generated clone text and TWSG002 the same. `Emit` itself is five writers: header, extension methods, clone methods, initializers, and holders.

Round-1 fixes stay on this id:

- Threading, tasks, `ICloneable`, `ReferenceAssemblyAttribute`, the shared BCL names, and `ValueTuple` match by name and namespace structure. A reference-pack build still treats `CancellationTokenSource` and `SemaphoreSlim` as shared copies. A null or polyfill symbol from `GetTypeByMetadataName` does not turn those checks off.
- A generator test compiles against the targeting pack and expects those two members to be reference copies with no TWSG002. The usual tests compile against implementation assemblies, where every `System.Threading` type shares one module, so they cannot see that bug.
- `Report` walks reachability only for roots that already produced a diagnostic, and skips that walk when none did.
- `scripts/e2e.cs` keeps its why-comments and Design text. The steps live in files the runfile includes, instead of being deleted to clear the line cap.
- Unused catalog members are gone, an empty metadata name never reaches `GetTypeByMetadataName`, the stale test comments name the files that exist, and `CorrectnessDiagnostics` sits with the correctness tests.

Acceptance: `dotnet fixie timewarp-state-source-generator-tests` passes, including the reference-pack case. `dotnet build scripts/e2e.cs` succeeds. `ganda repo audit` is clean.

## Checklist

- [x] Class made partial, no behaviour change, tests green
- [x] Extractions 1-8 landed (one PR per two or three extractions is fine)
- [x] Duplication and dead code removed
- [x] TypeCatalog caching in place
- [x] Subtype cache in place
- [x] Generated output byte-identical for the existing shape tests before and after
- [x] Round-1 M1–M6 fixed on this id
- [x] Reference-pack test treats CancellationTokenSource and SemaphoreSlim as shared copies
- [x] e2e comments restored; steps live in files the runfile includes

## Depends on

- 134

## Review

- Round 1: M1–M6 implemented on this id; round 2 re-reviews

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. The generator runs on `CompilationProvider` alone, so every keystroke replans; the caching items are what make that tolerable.

## Session

- Created: claude code review session (2026-11-04)
- Implementation: implementer oracle (2026-11-04)
- Boy scout: split the pre-existing files over the 500-line audit cap (analyzer, generator, state tests, and workflow-command). `scripts/e2e.cs` is a file-based app whose compile list does not include a sibling, so it was shortened to 500 lines in place. The named-element `IsTupleType` path stays: dropping it made `(string Name, Item It)` look like a hidden reference-assembly field and stopped emitting `ValueShapes.Clone`. `ganda repo audit --fix --checks kanban-landed` landed this kitchen on origin-home (`1dc7b6dc`). Re-audit has no errors. Advisory only: claim 098 has no worktree, and seven pre-existing paths are not kebab-case.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1a4c3-ad13-7253-8850-51702ca6d0fb, max-turns 500, Done — 2026-11-04T04:42:45Z
- Review oracle round 1: reviewer-claude (claude, model claude-opus-5-5), headless, effort 3, roster general (three scope passes as in-process subagents), turn cap 200. 6 findings open (2 bug, 3 suggestion, 1 nit), so a fix pass is needed — 2026-11-04
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session da172837-78bf-4ebc-a82f-36efeeb439b6, max-turns 200, artifacts general.md, merged.md — 2026-11-04T04:51:30Z
- Fix pass (implement-fix-2): namespace checks walk name and containing namespace, `Report` reaches only roots that already failed, `Emit` is five writers, and `scripts/e2e.cs` includes its step files instead of dropping comments. Task 136 still owns the dead `runMode`, Amuru exit codes, process lifetime, indentation, and `SutHost`. — 2026-11-04

## Learnings

- Misread (review round 1): the implementer restatement left out requirement 1's "Split `Emit` (164 lines) into header, extension methods, clone methods, initializers, holders", and `Emit` was moved without being split (review M4). It also turned "Generated clone text … stays the same" into "the existing generator tests are the check". Those tests compile against TPA implementation assemblies, so they could not catch the reference-pack namespace regression (M1).

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

`StateCloneTypeCatalog` holds the BCL and TimeWarp symbols the planner used to look up on every type (`MatchConcrete` was up to 24 lookups, `IsShared` about 9). Shared BCL names, `ICloneable`, `ReferenceAssemblyAttribute`, `ValueTuple`, and the `System.Threading` / `System.Threading.Tasks` namespaces match by walking the type name and each containing namespace. `GetTypeByMetadataName` is not that match, so a null lookup or a compilation polyfill does not drop the rule, and an empty metadata name never reaches the lookup. Namespace symbol equality is the wrong test on a reference pack: `CancellationTokenSource` and `SemaphoreSlim` are not in the same module as `Thread`. Collection and assignable checks still use symbol equality, because type symbols unify across modules. Non-generic subtype lists are cached per target. Assemblies that reference a declaring assembly are cached so `CandidateTypes` does not rescan modules. `Report` builds a reachability set only for roots that already produced a diagnostic, and it skips that walk when none did. Uncovered error roots are still reported in that empty case. `Emit` writes the header, extension methods, clone methods, initializers, and holders as five helpers, and the shape tests still lock the emitted text. The two closed-generic syntax scans share one walker. Field and auto-property copies share `AppendMember`. The two interface fallbacks share `MaterializeFallback`. `ICollection<T>` and `IList<T>` are no longer probed after `IEnumerable<T>`. The redundant pointer-symbol check is gone. `IsValueTuple` still accepts `IsTupleType` (a named tuple such as `(string Name, Item It)`); that path is the public element form, and the name-and-namespace walk covers `System.ValueTuple`N` after `TupleUnderlyingType` is unwrapped. `RememberICloneableState` uses a set. Unused catalog members (`ValueTuple`, `Named`, and the cached namespace symbols) are gone. `CorrectnessDiagnostics` lives with the correctness tests.

`scripts/e2e.cs` keeps its why-comments and Design text. The build, SUT, test, and dev-cert steps are partials the runfile includes (`e2e-build.cs`, `e2e-sut.cs`, `e2e-tests.cs`, `e2e-dev-certs.cs`). Task 136 still owns the dead `runMode`, Amuru exit-code handling, process lifetime, indentation, and `SutHost`. This id did not change those.

### How to validate

Smoke: `dotnet fixie timewarp-state-source-generator-tests`

Expect: 101 passed (7.91 seconds), including `Should_Share_Threading_Types_From_The_Reference_Pack`. That case compiles against the net11.0 reference pack, asserts the `System.Threading` namespace symbols are not equal, and expects `CancellationTokenSource` and `SemaphoreSlim` to be reference copies with no `TWSG002`. The shape tests still compile the generated clone and assert its text.

Smoke: `dotnet build scripts/e2e.cs`

Expect: build succeeds. The entry includes the four step files, so a missing method fails this build.

Smoke: `ganda repo audit`

Expect: exit 0. The repository passes with two pre-existing advisory warnings: `kanban-landed` (task 098 has no usable worktree) and `kebab-path-names` (seven paths that were already not kebab-case). Neither warning is a path this task changed.

### Review

- Round 1 (2026-11-04). Effort 3, roster general (three scope passes), reviewer-claude.
- Counts at review: bug 2, suggestion 3, nit 1.
- Disposition: M1, M2, M3, M4, M5, and M6 fixed on this id. Round 2 re-reviews; this node did not run a review.
- M1: threading and tasks match by namespace structure. The reference-pack test covers `CancellationTokenSource` and `SemaphoreSlim`.
- M2: e2e comments and Design text are restored. The steps moved into included files. Deeper e2e bugs stay on task 136.
- M3: `ICloneable`, `ReferenceAssemblyAttribute`, the shared BCL names, and `ValueTuple` use the same structural match.
- M4: `Emit` is `EmitHeader`, `EmitExtensionMethods`, `EmitCloneMethods`, `EmitInitializers`, and `EmitHolders`.
- M5: reachability is computed only for roots that already produced a diagnostic, and skipped when that set is empty.
- M6: unused catalog members removed, empty metadata names guarded, stale comments updated, `CorrectnessDiagnostics` moved.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.
- Gates run by review: generator tests 100 passed, analyzer tests 44/44, state tests 131 passed and 1 skipped, `dotnet build tools/dev-cli/dev.cs` OK.
- Gates run by this fix: generator tests 101 passed, `dotnet build scripts/e2e.cs` succeeded, `ganda repo audit` passed with the two advisory warnings above.
