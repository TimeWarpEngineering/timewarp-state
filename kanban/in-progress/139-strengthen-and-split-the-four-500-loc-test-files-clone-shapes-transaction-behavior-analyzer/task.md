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

## Goal (implementer restatement)

Make the four oversized test areas (source-generator clone shapes, StateTransactionBehavior, runtime generated
clone shapes, HandlerMustNotSendActionAnalyzer) catch real regressions and fit the line cap. Earlier line-cap
commits already split some files mechanically (`state-clone-shape-diagnostic-tests.cs`,
`state-transaction-behavior-fixtures.cs`, `generated-clone-shape-more-tests.cs`,
`handler-must-not-send-action-analyzer-negative-tests.cs`), so the line references in the description are stale.
The requirements still apply to the code wherever it now lives.

Acceptance:
- Every TWSG002 case asserts the diagnostic id and the offending member or message, not just "some error".
  The duplicate PrivateNestedSubclass run is gone. A guard fails when a shape dictionary key has no `[Input]`.
  New negatives cover `[GenerateClone]` with no members and `[CloneShared]` on a non-field member.
- Runtime clone assertions cover records, a collection subclass, a private constructor, frozen and immutable sets
  and queues, ConcurrentDictionary, and `[field: CloneShared]`. They also cover null and default members
  (null single, null box, empty ImmutableArray, default tuple). The `LogInformation` call gets an assertion.
- In StateTransactionBehavior tests, nothing is asserted inside the handler delegate: values are captured there and
  asserted after `Handle`. `CreateHarness` takes options, and one behavior factory replaces `CreateBehavior` and
  `CreateThrowBehavior`. Overlap tests use `RunContinuationsAsynchronously` TCS plus a timeout helper. New tests cover
  rethrow=true with OperationCanceledException, a plain success with replaced state, and explicit default
  no-rethrow. `ServiceState` and `TestNavigationManager` live in one shared fixtures file.
- Analyzer tests use marker-based spans (no hard-coded line/column) and `AnalyzerTestFactory` everywhere. New
  negatives cover `ISender.Send` of a non-IAction, a send outside a StateActionHandler, a call to a non-generated
  state method, `[AllowActionSend]` on a method, and TWS0003 with no send. A test pins whether `.g.cs` files get
  special handling.
- Each area is split by concern, and no file in these areas is over 400 lines.
- The analyzer, source-generator, and state test suites are all green, and `ganda repo audit` passes.

## Checklist

- [x] TWSG002 assertions check id/message per shape
- [x] Dictionary/Input guard
- [x] Runtime coverage for the listed compile-only shapes
- [x] Transaction delegate assertions moved out; harness takes options; TCS timeout helper
- [x] Shared fixtures file
- [x] Analyzer spans marker-based; negatives added
- [x] All four files split; no file over 400 lines
- [x] All tests green

## Review

- Round 1: Blocked — open M1, M2

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. `StateCloneGeneratorTestDriver` re-reads every TPA assembly on each run (~45x per file); caching the references is an optional extra here.

## Session

- Created: claude code review session (2026-11-04)
- 2026-11-05 implement (implementer-claude, headless): the line references in the description were stale, because
  earlier line-cap commits had already split part of each area. Three helpers worked in parallel, one per test
  project. I checked the combined result with a full `./bin/dev build`, the three Fixie suites, and `ganda repo audit`.
  The audit passes. Its two advisory warnings were already there and are out of scope: kanban-landed for a stale
  claim on task 098, and kebab-path-names for Blazor `.lib.module` asset names that must keep that form.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-sonnet-5-5), session 25d17f88-40fa-48cc-a678-746fa84e6269, max-turns 500, Done — 2026-11-05T11:16:28Z
- Review: cursor, general, effort 3, turn cap 200, round 1 open (2026-11-05). No separate vendor session id.
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 200), artifacts general.md, merged.md — 2026-11-05T11:24:00Z
- Fix pass (implement-fix-2, implementer-claude, claude-opus-5-5) for round 1, 2026-11-05: fixed M1 and M2. State tests 146 passed (1 skipped), source generator 109 passed. `ganda repo audit` shows only the two warnings already noted above (098 claim, `.lib.module` names).

## Results

All four areas are split by concern. No file in them is over 273 lines, and the cap is 400.

- **Source generator** (`tests/timewarp-state-source-generator-tests`). `state-clone-shape-tests.cs` is gone.
  - The shape sources are four partial `StateCloneShapeSources` files, one each for generics, collections,
    constructors and records, and CloneShared plus ICloneable. Each group has its own test class, and there are
    separate emitted-text, TWSG002, metadata and guard files.
  - Every TWSG002 case asserts exactly one TWSG002 at a source location, with message fragments that name the
    member and the reason, or the subtype.
  - The guard test reflects over the `[Input]` attributes. It fails on a missing Input, an extra Input, a repeated
    Input, or a duplicate key, and when a TWSG002 case has no message fragments.
  - The duplicate PrivateNestedSubclass run is gone.
  - New negatives: an empty `[GenerateClone]` gives no diagnostic and emits `new()`. `[CloneShared]` on a method gives
    only CS0592, because the attribute allows fields and properties only, and the generator ignores the method.
  - The test driver now builds the platform metadata references once in a static Lazy. The suite went from about
    10s to about 4s.
- **Transaction behavior** (`tests/timewarp-state-tests/pipeline`). It is split into rollback/cancellation/rethrow,
  clone-validation, overlap, injected-services, fixtures and fakes files.
  - One generic `CreateHarness<TAction>(state, configure, logger)` replaces `CreateBehavior` and
    `CreateThrowBehavior`.
  - Handler delegates only capture values now, and the assertions run after `Handle`. The `LogInformation` call is
    asserted through a `RecordingLogger` ("tick 0" and "tick 1").
  - Every TaskCompletionSource uses `RunContinuationsAsynchronously`, and every await goes through
    `WithTimeout` (10s), which is in `shared/task-timeout-extensions.cs`.
  - New tests: rethrow=true with an OperationCanceledException, which rethrows, rolls back and publishes nothing; a
    success path with a replaced state; and default options with no rethrow.
- **Shared fixtures**: `ServiceState` and `TestNavigationManager` are now in `tests/timewarp-state-tests/shared/
  service-state-fixtures.cs` (namespace `SharedTestFixtures`), used by both the pipeline and cloning tests.
- **Runtime clones** (`tests/timewarp-state-tests/cloning`). It is split into generics/null,
  collections/polymorphism/hierarchies, and constructors/ICloneable/services tests, plus two fixtures files.
  - New runtime coverage: a record class and a record struct, a collection subclass (its own `Tag` member is
    checked), a private constructor, ImmutableQueue, ImmutableHashSet, FrozenSet, FrozenDictionary,
    ConcurrentDictionary, and `[field: CloneShared]`.
  - New null/default cases: a null single member, a null box, a null list, an empty and a default ImmutableArray,
    and default tuples.
- **Analyzer** (`tests/timewarp-state-analyzer-tests`). It is split into TWS0002 positives, negatives, an allow file
  for TWS0003 and `[AllowActionSend]`, a multi-file file for partial and `.g.cs` cases, and a sources file.
  - Spans use `{|#0:...|}` markup with `WithLocation(0)` and the arguments, so no literal line or column is left.
  - `AnalyzerTestFactory.Create` has a multi-source overload, and nothing builds `CSharpAnalyzerTest` by hand.
  - New negatives:
    - an `ISender.Send` of an `IRequest<int>`
    - a send from a service class that is not a handler
    - a handler calling a hand-written state method
    - `[AllowActionSend]` on a method, which suppresses TWS0002 and reports TWS0003
  - TWS0003 reports on every `AllowActionSend` attribute on a handler, even when the handler does not send. This
    is pinned as it behaves today.
  - `.g.cs` decision: the analyzer uses `GeneratedCodeAnalysisFlags.None`, so a handler body inside a `.g.cs` file
    is not reported. A user handler that calls a generated entry is still reported. Both cases are pinned by tests.
- Test counts: analyzer 44 to 51, source generator 101 to 109, state 131 to 146 (1 skipped, unchanged). No product
  bugs were found, and `source/` is untouched.

Round 1 review (effort 3, roster general) found 1 bug and 1 nit. Both are fixed in the fix pass (see `review/round-1/merged.md`):

- M1: `Share_Injected_Services_By_Reference` now gives the clone a `RecordingLogger<ServiceState>`. It asserts that the clone's
  `LogInformation` call wrote exactly one Information entry with the message "clone kept the logger". The reference-equality
  checks stay. `RecordingLogger` and `LogEntry` moved from the transaction fakes to `tests/timewarp-state-tests/shared/
  recording-logger.cs` (namespace `SharedTestFixtures`), so the pipeline and cloning tests share one copy.
- M2: the Design region in `state-clone-shape-emitted-text-tests.cs` now points at `generated-clone-generics-tests.cs`,
  `generated-clone-collections-tests.cs` and `generated-clone-constructors-tests.cs`.

The goal restatement matches the original task; there is no misread.

### How to validate

Smoke:
```bash
./bin/dev build -q
dotnet fixie timewarp-state-analyzer-tests -c Release --no-build
dotnet fixie timewarp-state-source-generator-tests -c Release --no-build
dotnet fixie timewarp-state-tests -c Release --no-build
wc -l tests/timewarp-state-tests/pipeline/state-transaction-behavior-*.cs tests/timewarp-state-tests/cloning/*.cs \
  tests/timewarp-state-source-generator-tests/state-clone-*.cs tests/timewarp-state-analyzer-tests/handler-must-*.cs
grep -rn "WithSpan" tests/timewarp-state-analyzer-tests/handler-must-*.cs
grep -n "ShouldHaveSingleItem\|RecordingLogger" tests/timewarp-state-tests/cloning/generated-clone-constructors-tests.cs
```

Expect: the build succeeds. The suites report 51 passed, 109 passed, and 146 passed with 1 skipped. No file in the
`wc -l` list is over 400 lines. The `WithSpan` grep finds nothing. The last grep shows the
`RecordingLogger<ServiceState>` and the `ShouldHaveSingleItem` log assertion in `Share_Injected_Services_By_Reference`.
