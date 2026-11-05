# Strengthen and split the four 500+ LOC test files (clone shapes, transaction behavior, analyzer)
## Description

Four test files exceed 500 LOC and a review found weak assertions, duplicated fixtures, hang risks and overlapping coverage. Strengthen the assertions and split each file by concern.

Files:
- tests/timewarp-state-source-generator-tests/state-clone-shape-tests.cs (796)
- tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs (725)
- tests/timewarp-state-tests/cloning/generated-clone-shape-tests.cs (529)
- tests/timewarp-state-analyzer-tests/handler-must-not-send-action-analyzer-tests.cs (512)

## Requirements

state-clone-shape-tests.cs:
- TWSG002 table (~570-581) only asserts `errors.ShouldNotBeEmpty()`. Assert the diagnostic id, message or member for each of the 18 shapes.
- Shape dictionaries and `[Input]` lists are separate; add a guard that every dictionary key has an Input.
- ImmutableAndFrozen, OtherBclCollections, CollectionSubclass, Records, PrivateConstructor, NameCollision and the 8-tuple are compile-only. Add runtime clone assertions (in the runtime file) for Records, CollectionSubclass, PrivateConstructor, frozen/immutable sets and queues, ConcurrentDictionary, `[field: CloneShared]`.
- Remove the duplicate PrivateNestedSubclass run (~583-590).
- Add negatives: `[GenerateClone]` with no members; `[CloneShared]` on a non-field member.
- Split: supported-shape table (~14-336, by generics / collections / constructors+records / CloneShared+ICloneable), emitted-text checks (~338-422), TWSG002 (~425-617), metadata/reference assemblies (~619-796).

state-transaction-behavior-tests.cs:
- Assertions at ~430-433 run inside the handler delegate; the behavior catches and rolls back, so failures surface as a Count mismatch. Capture and assert after `Handle`.
- `CreateHarness` should take options so the Rethrow test (~107-124) stops hand-building. Merge `CreateBehavior` / `CreateThrowBehavior`.
- Add: RethrowHandlerExceptions=true with OperationCanceledException; plain success path with replaced state; explicit default no-rethrow assertion.
- Overlap tests (~209-410) use TaskCompletionSource without `RunContinuationsAsynchronously` and no timeout. Add a timeout helper.
- Split: rollback/cancellation/rethrow, clone-validation, overlap, injected services, shared fakes.
- `ServiceState` and `TestNavigationManager` are duplicated with generated-clone-shape-tests.cs (~496-528 vs ~598-634). Share them.

generated-clone-shape-tests.cs:
- Line ~233 `LogInformation` has no assertion.
- Add null/default member cases (`Single=null`, `Box=null`, empty ImmutableArray, default tuple).
- Split: generics/tuples/nested, collections/polymorphism/hierarchies, ICloneable/BCL/constructors/services, fixtures to shared file.

handler-must-not-send-action-analyzer-tests.cs:
- Hard-coded line/column spans at ~58, 106, 146, 207, 275, 336, 507 break on whitespace. Use a marker-based span helper.
- Only two negatives. Add: `ISender.Send` of a non-IAction; send outside a StateActionHandler; call to a non-generated state method; `[AllowActionSend]` on a method; TWS0003 with no send.
- ~266-278 and ~327-337 hand-build the analyzer test; use `AnalyzerTestFactory`. Decide and test whether `.g.cs` files are special-cased.
- Split: TWS0002 positives, negatives, TWS0003, source-string constants.

## Checklist

- [ ] TWSG002 assertions check id/message per shape
- [ ] Dictionary/Input guard
- [ ] Runtime coverage for the listed compile-only shapes
- [ ] Transaction delegate assertions moved out; harness takes options; TCS timeout helper
- [ ] Shared fixtures file
- [ ] Analyzer spans marker-based; negatives added
- [ ] All four files split; no file over 400 lines
- [ ] All tests green

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. `StateCloneGeneratorTestDriver` re-reads every TPA assembly on each run (~45x per file); caching the references is an optional extra here.

## Session

- Created: claude code review session (2026-11-04)
