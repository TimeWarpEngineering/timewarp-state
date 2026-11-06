# Option 3b (State side): emit declared handlers on the State graph

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 3b, State half. Mediator half is a timewarp-mediator task filed the same day (`GeneratedHandlerAttribute`).

State adds `ReducerAttribute : GeneratedHandlerAttribute` (HandlerTypeName "Handler") and emitters on the Option 1 graph that generate the handler Mediator will dispatch to. A State analyzer reports a declared handler the generator did not emit before Mediator's CS0246 can.

## Requirements

- Depends on the Mediator release that ships `GeneratedHandlerAttribute`: **TimeWarp.Mediator 15.0.0-beta.5**, released 2026-11-06 (Mediator task 029). Bump the pin to it in the same commit.
- Emitters on the Option 1 graph; no second scan.
- Analyzer for the missing-handler case with release tracking.
- Tests: generated handler dispatches end to end in the test app.

## Goal (implementer restatement)

Let an action set skip its hand-written `Handler`. The user marks the nested `Action` with
`[Reducer]` and writes one static `Reduce(TState state, Action action)` method in the action set.
`ReducerAttribute` derives from Mediator's `GeneratedHandlerAttribute` and names `Handler`.
Mediator 15.0.0-beta.5 then dispatches the Action to `XActionSet.Handler`. The State generator
emits that `Handler` from the existing State graph (the same `ActionFact`, no second scan).

Acceptance criteria:

- `ReducerAttribute` ships in TimeWarp.State. It takes a constructor parameter named
  `handlerTypeName` with default `"Handler"`, as Mediator's cross-assembly contract requires.
- The generator emits `XActionSet.Handler : StateActionHandler<Action>` for each valid
  `[Reducer]` action. It is a new emitter on the Option 1 graph with its own tracking name.
- A State analyzer reports a `[Reducer]` the generator will not serve, with the reason, on the
  attribute. Mediator's CS0246/TWM012 still fires, but the State diagnostic says what to fix.
  The rule is shared source in `facts/`, so generator and analyzer cannot drift. Release tracking
  is updated.
- A `[Reducer]` action in the test app dispatches end to end through the generated Mediator and
  changes state, proven by a test.
- TimeWarp.Mediator and TimeWarp.Mediator.Generators are pinned to 15.0.0-beta.5 in the same
  commit. The TimeWarp.State version is bumped. The build has 0 warnings and 0 errors, tests pass,
  and `ganda repo audit` is clean.

## Checklist

- [x] Attribute and emitters
- [x] Analyzer
- [x] End-to-end test; build 0/0 (no new warnings); pin bump; version bump (12.0.0-beta.17 is unreleased, so the note joins it)

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)
- 2026-11-06: implementer (ganda task work, implement oracle). Pinned Mediator 15.0.0-beta.5, added `[Reducer]`,
  the reducer emitter on the State graph, TWS0014, and tests. `ganda repo audit` passes. Its two advisories
  (kebab names that Blazor requires, and task 098's claim with no worktree) were there before this task and belong
  to other work.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session bf79c77b-615e-4401-ba18-b9d341443293, max-turns 500, Done — 2026-11-06T02:44:19Z
- Review oracle: tw-implementation-review at effort 3 (roster: general), by claude-opus-5-5, session
  f0bc6949-86e9-491b-b3c0-f2827586af95, turn cap 200. Round 1 → accepted-exceptions, 2026-11-06.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session f0bc6949-86e9-491b-b3c0-f2827586af95, max-turns 200, artifacts general.md, merged.md, disposition.md — 2026-11-06T02:48:12Z

## Results

TimeWarp.State now emits the handler for a `[Reducer]` action, and TimeWarp.Mediator 15.0.0-beta.5 dispatches
to it.

**What the developer writes**

```csharp
public static partial class DecrementCountActionSet
{
  [Reducer]
  public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

  private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
}
```

**What changed**

- `source/timewarp-state/attributes/reducer-attribute.cs`: `ReducerAttribute(string handlerTypeName = "Handler") :
  GeneratedHandlerAttribute`. The name is a constructor parameter because Mediator reads it from a reference
  assembly, and a reference assembly keeps no getter body.
- `source/timewarp-state-source-generator/facts/reducer-facts.cs`: the shared rule (placement, generic, abstract
  state, `IAction`, partial chain, accessibility, simple name, no hand-written member of that name, exactly one
  `static void Reduce(TState, Action)`). It is linked into the analyzer, so the analyzer and the generator cannot
  drift.
- `emitters/reducer-handler-emitter.cs` plus `ActionFact.Handler`, `StateGraph.ReducerHandlers` and the new
  tracking step `State.Emit.Reducer`. The handler comes from the same action fact the dispatch method uses, so
  there is no second scan. The emitted `Handler` is `global::`-qualified, has the Action's accessibility, and
  carries `[GeneratedCode("TimeWarp.State.SourceGenerator", "1.0.0")]` so the analyzer does not mistake it for a
  hand-written one.
- `source/timewarp-state-analyzer/reducer-analyzer.cs`: **TWS0014** (Error) on the attribute, with the reason.
  Release tracking is in `AnalyzerReleases.Unshipped.md`. The analyzer readme, `documentation/topics/analyzers.md`
  and the unreleased `release12.0.0-beta.17.md` are updated.
- Pin: `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` move to 15.0.0-beta.5 in `Directory.Packages.props`.
  The living docs and the nuspec allow-list test are updated to match.
- Version: `12.0.0-beta.17` is not on NuGet yet (`./bin/dev check-version` reports it as one increment ahead and
  safe to release). This change goes into that release note rather than a second bump.
- Test app: `CounterState.DecrementCountActionSet` uses `[Reducer]` and has no hand-written handler.

**Gates run**

- `./bin/dev build`: exit 0, 0 errors. The warnings are the existing baseline (TW0007, CS8600/8602, NU19xx, and
  others). This change adds no warning: the one TW0007 the new test introduced was fixed by moving the using into
  global usings.
- `./bin/dev test`: exit 0, all 9 suites pass. That includes 11 new analyzer tests, 10 new generator tests
  (incrementality among them), and 3 new client integration tests.
- `./bin/dev verify-samples`: exit 0.
- `ganda repo audit`: passes, with the 2 advisories noted under Session.

**Follow-ups (not in scope)**

- An async reducer (`ValueTask Reduce(..., CancellationToken)`) and reducers that need services. Option 5's
  state-instance `Reduce` is also not covered.
- The State manifest does not yet mark which actions use a generated handler.

### Review disposition

- **Rounds:** 1. **Effort:** 3 (budget by diff, 1288 lines). **Roster:** general.
- **Final counts:** bug 0, suggestion 0, nit 1. Nothing is open or fixed; the nit is wontfix.
- **Disposition:** `accepted-exceptions`. M1 (nit, wontfix): TWS0014 checks only the Action's declared accessibility.
  A public Action inside a private or protected action set fails in Mediator.g.cs with CS0122, not with TWS0014. A
  hand-written handler fails the same way, so this is no regression, and the fix belongs with the next TWS0014 change.
- **Reviewer gates:** `./bin/dev build` exits 0 with 0 errors. These test projects pass: client-integration-tests (67
  passed, 1 skipped), timewarp-state-analyzer-tests (77) and timewarp-state-source-generator-tests (174). The generated
  `Handler_Reducer.g.cs` exists in the test app. NuGet's latest is 12.0.0-beta.16, which confirms beta.17 is unreleased.
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`,
  `review/disposition.md`.

### How to validate

Smoke:

```bash
./bin/dev build
dotnet test tests/client-integration-tests/client-integration-tests.csproj --no-build -v n | grep ReducerDispatch
dotnet test tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj --no-build -v n | grep ReducerAnalyzer_
dotnet test tests/timewarp-state-source-generator-tests/timewarp-state-source-generator-tests.csproj --no-build -v n | grep ReducerHandlerGenerator_
cat tests/test-app/test-app-client/generated/timewarp-state-source-generator/*/Test.App.Client.Features.Counter.CounterState+DecrementCountActionSet.Handler_Reducer.g.cs
```

Expect: the build exits 0. The three `ReducerDispatch.DecrementCount_Should` tests pass: a send through
`ISender<ClientPipeline>` takes Count from 10 to 6, and `DecrementCount(3)` takes it from 10 to 7. All 11
`ReducerAnalyzer_` tests and all 10 `ReducerHandlerGenerator_` tests pass. The generated file declares
`public sealed class Handler(global::TimeWarp.State.IStore store) :
global::TimeWarp.State.StateActionHandler<...DecrementCountActionSet.Action>(store)`, and its `Handle` calls
`...DecrementCountActionSet.Reduce(Store.GetState<...CounterState>(), action)`. Removing `partial` from
`DecrementCountActionSet` makes the build fail with TWS0014 on `[Reducer]`, which names the reason.

## Depends on

- Option 1 task; timewarp-mediator Option 3b task

## Review

- Round 1: Done

## Notes

Steve 2026-11-06 decision; order 0 → 1 → 3b.
