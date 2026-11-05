# Option 0: fix the State generators in place

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 0 from Steve's 2026-11-06 decision (0 → 1 → 3b → 5 → 4; 2 any time).

Fix the four generators where they are. No new graph yet (that is Option 1).

## Requirements

- ActionSet method generator: equatable record model with `WithTrackingName` on every step; semantic discovery (parent derives from `State<T>`, Action implements `IAction` or is a nested `Action` class or record); read primary constructors and positional records; `required`/`init` properties as parameters; drop SG002; report a TWS diagnostic instead of emitting broken code; direct tests.
- Catalog generator: widen the predicate to `TypeDeclarationSyntax`; the shared constructor parser reads a primary-constructor or positional-record parameter list before an explicit constructor.
- Persistence generator: match `[PersistentStateAttribute]` too, or use `ForAttributeWithMetadataName`.
- Every generator: an incrementality test in the Mediator style.
- **Analyzer contract (gap found after the 140 review):** add a table to the kitchen mapping every assumption a generator makes about its input to the TWS rule that guards it. For each unguarded assumption decide analyzer or generator fix. Today no TWS rule covers action constructors, so a primary-constructor Action reaches the generator and emits CS7036. Where the right fix is a rule, add it with release tracking; where it is the generator, fix the generator. Record the table in a Design region of the generator it describes.
- **Ramani (gap found after the 140 review):** inventory `timewarp-taratibu/source/timewarp-ramani-generators/` (locators, interpreter, emitters) the way 140 did Nuru and Mediator, and record in the kitchen anything it does that should change this task's shape.
- Not breaking: code that failed before now compiles; nothing that compiled changes meaning.
- Version bump per the repo's release rule.

## Goal (implementer restatement)

Make the four State source generators correct and measurably incremental where they stand, without the
single-graph rewrite (that is Option 1, a later task).

Acceptance criteria, in my words:

1. **ActionSet method generator** builds an equatable record model (no `List<>`, no symbols, no spans on the
   success path), names every pipeline step, finds action sets semantically (the containing type derives from
   `State<T>` or implements `IState`; the action is the nested `Action` class, record or struct), reads a primary
   constructor or positional record parameter list before an explicit constructor, turns `required` members into
   parameters, no longer emits SG002, and reports a tracked `TWSG` diagnostic instead of emitting code that cannot
   compile. It gets its own direct tests (shape, compile, diagnostics, incrementality).
2. **Catalog generator** accepts record actions (predicate widened to `TypeDeclarationSyntax`) and gets its
   parameters from the same shared parser, so primary-constructor and positional-record actions get executors.
3. **Persistence generator** also matches `[PersistentStateAttribute]` written out in full.
4. **Every generator** (ActionSet, catalog, persistence, clone) has named steps and a Mediator-style test: an
   unrelated edit leaves the named steps `Cached`/`Unchanged` (or the test pins and documents what reruns), and a
   relevant edit re-emits.
5. **Analyzer contract:** a table (in this kitchen and in the Design region of each generator it describes) of every
   input assumption each generator makes, the TWS/TWSG rule that guards it, and for each unguarded one an explicit
   decision: analyzer rule, generator fix, or deliberately left with a reason. New rules carry release tracking.
6. **Ramani inventory:** `timewarp-taratibu/source/timewarp-ramani-generators/` inventoried like 140 did Nuru and
   Mediator, and anything that should change this task's shape recorded here.
7. **Not breaking:** code that failed before may now compile; nothing that compiled changes meaning (no signature
   change for any action shape that already produced a compiling method). *Fix pass 1 (review round 1):* this also
   covers code that compiled **because the old generators ignored it**. "Served before" means a plain class Action
   in a class state (method generator) and a class Action (catalog). Only those shapes may get TWSG003. Newly served
   shapes that cannot get a method are skipped silently. A plain class Action keeps "first explicit constructor"
   ahead of the primary constructor. A hand-written method is cataloged only when the executor's call binds to it.
8. Version bumped per the repo rule (`12.0.0-beta.16` is published, so `12.0.0-beta.17` with release notes), build
   0 warnings / 0 errors, tests green, `ganda repo audit` clean, committed and pushed on the task branch.

## Checklist

- [x] Analyzer-contract table and Ramani inventory in the kitchen
- [x] ActionSet generator fixed with tests
- [x] Catalog and persistence generators fixed
- [x] Incrementality tests on every generator
- [x] Build 0 errors (93 pre-existing warnings, none in changed files; see Results), tests green, release tracking updated

## Analyzer contract

Every assumption a State generator makes about its input, what guards it, and the decision for the ones nothing
guarded before. The same table, per generator, is in that generator's `#region Design`.

### ActionSet method generator (`action-set-method-generator.cs`, shape rules in `action-set-shape.cs`)

| Assumption | Guard before | Decision (guard now) |
|---|---|---|
| The parent of `*ActionSet` is a state | none (docs class got broken `partial class Docs`) | generator: semantic check, parent implements `IState`; otherwise skip silently |
| An `IAction` is nested in an `IState` | TWS0001 | unchanged |
| (every TWSG003 row below) | | only for shapes served before: a plain class Action in a class state (`ActionSetShape.WasServedBefore`). A record or struct Action, or an ActionSet in a record or struct state, compiled before because it got no code, so a failure there is skipped silently (review M1) |
| The state and every containing type are `partial` | none (CS0260) | generator: TWSG003 |
| The ActionSet has a prefix (not just `ActionSet`) | none (empty method name) | generator: TWSG003 |
| Action and ActionSet have no own type parameters | none | generator: TWSG003 (a generic state is fine now) |
| Action is not abstract | none (CS0144) | generator: TWSG003 |
| Action and its chosen constructor are reachable from the state | none (CS0122) | generator: TWSG003 |
| Constructor shape: primary, positional record, explicit, none | none (records ignored, primary ctor CS7036) | generator: reads all four. Record/struct: primary first. Plain class: first explicit first, then primary, which keeps the old signature (review M2) |
| `required` members are set | none (CS9035) | generator: parameters + object initializer; skipped with `[SetsRequiredMembers]` |
| Plain `init` properties | n/a | none, by design: adding them would change a compiling signature |
| Parameter names unique and not `externalCancellationToken` | none (CS0100) | generator: TWSG003 |
| No non-method state member named like the method | none (CS0102) | generator: TWSG003 |
| The state does not already declare the method | none | generator: emits nothing; a record/struct Action defers to any same-name method |
| Parameter types at least as visible as the method | none (CS0051) | generator: `internal` method on a public state, TWSG003 for private types |
| Default values bind in the state's scope | none (CS0103) | generator: emits the bound constant when the source text is not a literal |
| Nested, generic, global-namespace states | none (broken partial) | generator: containing chain, type parameters, namespace from symbols |
| The Action is named exactly `Action` | none | none, by design: other nested types get no method (Option 5 adds a second shape) |
| The Action implements `IAction` | `Sender.Send` constraint | none: the compiler reports it on the user's type |
| The state has `Sender` and `CancellationToken` | none | none: an `IState`-only state behaves exactly as before |

### Action catalog generator

| Assumption | Guard | Decision |
|---|---|---|
| `[CatalogAction]` on a nested `*ActionSet.Action`, none generic, all at least internal | TWS0004 | unchanged; TWS0004 already accepts record classes (`TypeKind.Class`) |
| Description present, one plain sentence; DisplayName not blank; names unique | TWS0005, TWS0007, TWS0008, TWS0006 | unchanged |
| The Action may be a record or have a primary constructor | none (record invisible, primary ctor had no parameters) | generator: `TypeDeclarationSyntax` predicate + the shared parser |
| The generated `State.Method` exists with the parser's parameters | none (follow-on CS1061) | generator: shares `ActionSetShape`; skips the entry when the method cannot be generated |
| A hand-written `State.Method` takes the executor's call | none (CS1739, CS1501) | generator: entry kept only when the method takes the generated parameter types plus a trailing `externalCancellationToken`, returns `Task`, and has defaults where the generated one does (`UserMethodCallable`); else skipped (review M3) |
| The parent implements `IState` (`GetState<T>` constraint) | none | generator: skip |

### Persistence generator

| Assumption | Guard | Decision |
|---|---|---|
| The type is top level | TWSG001 | unchanged |
| The type is `partial` | none (CS0260) | generator: TWSG003 |
| The type derives from `State<T>` | none (CS0103 on Sender) | generator: TWSG003 when `State<T>` is referenced |
| The attribute is spelled `PersistentState` | name match only | generator: also `PersistentStateAttribute` (qualified or not); still a name match so nothing that matched stops matching |
| The state is public | none (CS0262 for internal) | generator: partial without an accessibility modifier |
| Consumer has `System.Threading` usings | none | generator: `global::` names |

### Clone generator

| Assumption | Guard | Decision |
|---|---|---|
| A concrete `State<T>` has `ICloneable` or a callable constructor | TWS0009 | unchanged |
| `T` is the deriving class | TWS0010 | unchanged |
| Every reachable member can be cloned or is `[CloneShared]` | TWSG002 | unchanged |
| Public state properties are read-only | TWS0012 | unchanged |

**No new TWS analyzer rule.** For every unguarded assumption, the generator was the better owner. It is the only
component that knows exactly what it will emit, and an analyzer would have duplicated `ActionSetShape`, which is the
two-graph-builder cost task 140 warned about. One new generator rule, **TWSG003**, is in
`source/timewarp-state-source-generator/AnalyzerReleases.Unshipped.md`.

## Ramani inventory

`timewarp-taratibu/source/timewarp-ramani-generators/`, 11,652 LOC. It has one generator and one syntax-provider
pipeline. A helper agent did the read-only inventory. I spot-checked `ramani-generator.cs:14-18,60-70`,
`models/location-info.cs:27-37` and the LOC total against the summary.

- **Inputs:** `CreateSyntaxProvider` for any zero-argument `.Build()` call. A semantic check binds it to
  `StateMachineBuilder<,>.Build`, then walks back to `Named`. `CompilationProvider` is used only for diagnostics
  (`ramani-generator.cs:63-98`, `locators/machine-chain-locator.cs:45-75`).
- **Stages:** `locators/` (292 LOC, placement and usings), `interpreter/` (2,706, a semantic walk of the builder
  calls that fails fast), `validation/` (483, pure checks on the model), `emitters/` (6,590: the machine, orthogonal
  regions, Mermaid, and the definition hash). The flow is syntax provider, then `.Collect()`, then `Resolve`
  (cross-machine checks), then one source output plus one diagnostics output combined with the compilation.
- **Model:** sealed records over strings, `EquatableArray<T>` and `LocationInfo`, with no symbols or syntax.
  Diagnostics are equatable `DiagnosticInfo` values, reported in a second output that finds the real tree again
  (`location-info.cs:27-37`), so `#pragma` and `.editorconfig` severities apply. Any Error suppresses emission.
- **Gaps:** no `WithTrackingName` and no incrementality test. Spans in the model invalidate it on edits above it.
  One `.Collect()` output re-emits every machine. The `SM` diagnostic prefix is generic.
- **Tests:** Jaribu with an in-process `CSharpGeneratorDriver`, snapshot files (`*.verified.txt`, no Verify
  package), and diagnostics tests per phase.
- **Shared code:** none, on purpose (decision 060-001(c): copy Nuru's `EquatableArray` and patterns until a third
  generator needs them).

**What changed this task's shape:**

1. Diagnostics are carried as equatable data and re-joined to the real tree in a separate output (`LocationInfo`,
   `GeneratorDiagnosticInfo`). The ActionSet and persistence generators now do this instead of
   `Location.Create(path, …)`.
2. The success model holds no span (Ramani's weakness), so an edit above the action in the same file stays
   cached. `Given_Edit_Above_The_Action_In_The_Same_File` pins this.
3. Per-item outputs where no cross-item view is needed (ActionSet method, persistence), not one `.Collect()`.
4. "Any error means no emit" is the TWSG003 rule.
5. No shared generator helper package. That matches Ramani's 060-001(c). State's own `EquatableArray` stays local.

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)
- 2026-11-06 implementer (claude-opus-5-5, ganda task work implement node): goal restatement, generator fixes,
  tests, contract table, Ramani inventory (read-only helper agent), version bump to 12.0.0-beta.17.
- The full build showed the generator's first draft broke the test app:
  `ApplicationState.FiveSecondTask` is a hand-written method for a record Action that the old generator ignored. That
  produced CS0111. The rule "the state already declares the method, so emit nothing" came from this, and it is
  tested.
- Audit: `ganda repo audit` passes. Its two advisory warnings are pre-existing and not this task's to fix:
  `kanban-landed` (task 098's claim has no worktree) and `kebab-path-names` (Blazor JS module and MSBuild targets
  files whose names must match assembly names, plus backlog 110's double hyphen).
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session b97408ce-cef4-49ba-a641-6515790832be, max-turns 500, Done — 2026-11-05T19:06:35Z
- Review oracle round 1 (2026-11-06): general reviewer (claude, model claude-opus-5-5). Effort 3 by diff (2433
  lines), turn cap 200. Three open bugs, so a fix pass is needed. See `review/round-1/merged.md`.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 3fdb9824-f269-433f-8a52-0ae599a7950e, max-turns 200, artifacts general.md, merged.md — 2026-11-05T19:12:06Z

- 2026-11-06 implementer fix pass 1 (claude-opus-5-5, implement-fix-2 node): M1, M2 and M3 from
  `review/round-1/merged.md` fixed with 10 new tests. Generator suite 140 to 150. Full build and test rerun.

## Results

### What changed

- **ActionSet method generator** (`action-set-method-generator.cs`, `action-set-shape.cs`,
  `action-set-constructor-parser.cs`):
  - Semantic discovery. The parent must implement `IState`, and the Action is the nested `Action` class, record or
    struct.
  - Equatable `ActionSetMethodModel` and `ActionSetMethodResult` records, three named steps, and diagnostics
    re-joined to the tree.
  - Constructor choice in order: primary constructor or positional record, then the first explicit constructor.
    `required` members become parameters.
  - Nested, generic and global-namespace states merge.
  - SG002 is removed. TWSG003 is reported instead of emitting broken code.
  - A hand-written method on the state wins.
- **Catalog generator:** the predicate is now `TypeDeclarationSyntax`. Parameters and the skip decision come from
  the shared `ActionSetShape` and parser. Steps are named.
- **Persistence generator:** it also matches `[PersistentStateAttribute]`. It uses a semantic transform and
  equatable result records. It reports TWSG003 when the state is not partial or not a `State<T>`. The emitted
  partial has no accessibility modifier and uses `global::` names. Steps are named.
- **Clone generator:** the plan is a named `CompilationProvider.Select` that returns an equatable
  `ClonePlanResult`, so an equal plan leaves the output `Cached`. The planner collects diagnostics into a list
  instead of a `SourceProductionContext`.
- **Shared:** `tracking-names.cs`, and `generator-diagnostic-info.cs` (`LocationInfo`, `GeneratorDiagnosticInfo`,
  TWSG003). TWSG003 is in `AnalyzerReleases.Unshipped.md`.
- **Tests (new):**
  - `action-set-method-generator-tests.cs` and `action-set-method-generator-failure-tests.cs`, 30 cases after fix
    pass 1: the spike shapes, required members, state shapes, the hand-written-method cases, the TWSG003 cases, and
    the shapes skipped silently.
  - `generator-incrementality-tests.cs`, 9 cases across all four generators.
  - `generator-test-host.cs`.
- **Fix pass 1 (review round 1):**
  - **M1:** `ActionSetAnalysis.ServedBefore` (`ActionSetShape.WasServedBefore`) is true only for a plain class Action
    in a plain class state, the old predicate's shape. The method generator reports TWSG003 only when it is true. A
    failing record or struct Action, or an ActionSet in a record or struct state, is skipped with no diagnostic. That
    also applies to the "any same-name method" rule. Seven tests are in `Should_Skip_Silently_Shapes_Not_Served_Before`:
    a non-partial state, a property clash, the `externalCancellationToken` name, an abstract record, a private
    record, a private parameter type, and a class Action in a record state. The TWSG003 token test now uses a class
    primary constructor, which was CS7036 before.
  - **M2:** `ActionSetConstructorParser.SelectConstructor` takes the first explicit constructor before the primary
    one for a plain class Action. Records and structs keep primary first. The test is flipped to
    `Given_Class_With_Primary_And_Explicit_Constructors_Keeps_The_Explicit_Signature`, which expects `Set(string name, …)`.
    `Given_Record_With_Primary_And_Explicit_Constructors_Prefers_Primary` is new.
  - **M3:** `ActionSetAnalysis.UserMethodCallable` is computed in `ActionSetShape.IsCallableByCatalog`. The catalog
    skips a user-declared method unless that method is an instance method reachable from the assembly that takes the
    generated parameter types, ends in `externalCancellationToken`, returns `Task`, and has defaults where the
    generated method does. There are two new catalog tests. With another signature, no catalog is emitted and the
    code compiles. With the generated signature, the executor calls the user's method and the code compiles.
  - Docs: `analyzers.md`, `action-catalog.md`, the analyzer readme and the beta.17 release notes now describe the
    served-before scope of TWSG003, the constructor order and the catalog rule.
- **Tests (changed):** two persistence tests now assert `partial class` without `public`. The clone cancellation
  test passes a list.
- **Docs:** `documentation/topics/analyzers.md` (TWSG003), `documentation/topics/action-catalog.md` (records,
  parameter order), and `source/timewarp-state-analyzer/readme.md`.
- **Release:** version `12.0.0-beta.17` in `source/Directory.Build.props` and `msbuild/repository.props`
  (beta.16 is on NuGet), plus `documentation/release-notes/release12.0.0-beta.17.md` and its toc entry.

### Measured incrementality (Roslyn 4.14.0)

After an unrelated file is added, every named step is `Cached`, except two that are `Unchanged`: the catalog's
assembly-name step and the clone plan. Every output step is `Cached`, and the generated text is identical. After an
edit above the action in the same file, `ActionSet.Candidates` is `Cached`. **Task 140's open question is settled
for State:** `ForAttributeWithMetadataName` does not defeat caching here. `Catalog.Entries` is `Cached` after an
unrelated edit. The clone planner still runs on every compilation change. Moving it off `CompilationProvider` is
Option 1.

### Gates

- `./bin/dev build`: 0 errors and 93 warnings. Every warning is in a file this task does not touch: CS8600 and
  CS8602 in the clone tests, TW0007 usings, RS0030 `Console` in E2E, NU1510, NU1902 and NU1903 advisories, and BL
  and ASPDEPR warnings. None comes from a changed file. The checklist's "0/0" is therefore 0 errors, not 0 warnings,
  and this task did not raise the warning count.
- `./bin/dev test`: exit 0. Every suite passed. The generator suite went from 109 to 140 tests. The skips are
  pre-existing.
- `./bin/dev verify-samples`: passed.
- `./bin/dev check-version`: `12.0.0-beta.17` is new and safe to release.
- `ganda repo audit`: passes, with two pre-existing advisory warnings (see Session).
- **Fix pass 1 rerun (2026-11-06):**
  - `./bin/dev build` from a clean `generated/` folder: 0 errors and 156 warnings. That count is a full rebuild
    where MSBuild prints most warnings twice. None is in a file this task changed. They are the same families as
    above: RS0030 in E2E, CS8600 and CS8602 in the clone tests, TW0007, BL0016 and NU1510.
  - `./bin/dev test`: exit 0, with no failures. The generator suite has 150 passed.
  - `ganda repo audit`: passes, with the same two advisories. During the pass it flagged `source-file-line-cap`
    because `action-set-method-generator-tests.cs` reached 614 lines. I fixed that by moving the TWSG003 and
    skip-silently classes to `action-set-method-generator-failure-tests.cs`.
  - In the test app, `FiveSecondTaskActionSet_Method.g.cs` is still not generated, and the class Actions still get
    their methods.

### Decisions worth a reviewer's eye

- Plain `init` properties are **not** parameters. Adding them would change `IncrementCount()` in the test app and in
  any consumer, which breaks the non-breaking rule. Mark a member `required` to opt in.
- A record or struct Action defers to **any** same-name method on the state, not only an identical signature,
  because those shapes never had a generated method and a near overload could make existing calls ambiguous.
- No new TWS analyzer rule. TWSG003 lives in the generator. See the Analyzer contract section for the reasoning.
- TWSG003 is an Error, because every case it reports used to be a compiler error in generated code. After fix pass 1,
  that holds because it is reported only for shapes served before. A newly served shape that fails is silent, not an
  Info diagnostic. That keeps the change free of a new rule id, at the cost of discoverability; Option 1 can add an
  Info.

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-state/task-141-option-0-fix-the-state-generators-in-place-equatab
dotnet tool restore
(cd tests/timewarp-state-source-generator-tests && dotnet fixie timewarp-state-source-generator-tests)
rm -rf tests/test-app/test-app-client/generated   # gitignored; not cleaned between builds
./bin/dev build
./bin/dev test
```

**Expect**

- The generator suite prints `150 passed`, including `GeneratorIncrementality_.*` (9),
  `ActionSetMethodSourceGenerator_.*` (30, with `Should_Skip_Silently_Shapes_Not_Served_Before` at 7) and
  `ActionCatalogSourceGenerator_.Should_Respect_A_Hand_Written_Method` (2).
- `./bin/dev build` ends with `0 Error(s)`. The test app compiles, which includes the hand-written
  `ApplicationState.FiveSecondTask` beside its record Action.
- `./bin/dev test` exits 0.
- In `tests/test-app/test-app-client/generated/timewarp-state-source-generator/`, generated
  `*_Method.g.cs` files exist for class actions, with no `FiveSecondTaskActionSet_Method.g.cs` and no SG002 in
  the build log.

### Review

- **Round 1** (2026-11-06), effort 3, roster: general.
  - Final counts so far: bug 3 open, 0 fixed, 0 wontfix. Suggestion 0. Nit 0.
  - **Disposition: pending.** Fix pass 1 implemented M1, M2 and M3 (see "Fix pass 1" under What changed). Round 2
    decides the dispositions.
  - All three findings break the "Not breaking" requirement. Each was confirmed with a scratch consumer project
    outside the repo:
    - **M1:** TWSG003 errors on record and struct Actions the old generator ignored.
    - **M2:** a class Action with a primary constructor and an explicit one changes signature.
    - **M3:** a cataloged record Action with a hand-written method breaks the generated catalog.
- Gates the reviewer ran: generator suite `dotnet fixie`, 140 passed. The reviewer did not rerun the full
  `./bin/dev build` or `./bin/dev test`.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Learnings

- Misread (review round 1). The task says "Not breaking: code that failed before now compiles; nothing that compiled
  changes meaning." The restatement narrowed that to criterion 7: "no signature change for any action shape that
  already produced a compiling method". That wording leaves out code that compiled *because the generator ignored
  it*, which covers record and struct Actions and records that were never cataloged. Those shapes now fail with
  TWSG003 or CS1739 (M1, M3). The restatement also missed that the task's "primary constructor before explicit"
  ordering conflicts with "not breaking" for a class that has both constructors (M2).

## Review

- Round 1: Blocked — open M1, M2, M3
- Fix pass 1 (2026-11-06): M1, M2 and M3 implemented on this id. Dispositions are for review round 2.

## Notes

Steve 2026-11-06: all of 0, 1, 2, 3b, 4, 5; cockpit set the order. First of the family.
