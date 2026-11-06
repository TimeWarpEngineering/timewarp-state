# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** same as framework (branch vs master). Read in full: `source/timewarp-state/attributes/reducer-attribute.cs`,
`source/timewarp-state-source-generator/facts/reducer-facts.cs`, `emitters/reducer-handler-emitter.cs`,
`extractors/action-fact-extractor.cs` (diff plus `Keyword`/`Declaration` helpers), `graph/state-graph*.cs`,
`timewarp-state-generator.cs`, `tracking-names.cs`, `source/timewarp-state-analyzer/reducer-analyzer.cs` and csproj link,
`AnalyzerReleases.Unshipped.md`, the test-app `[Reducer]` action, the client integration test, the test name lists of the
analyzer and generator suites, docs, and the release note. Also read `base/state-action-handler.cs`.

## Summary

The change adds `ReducerAttribute(string handlerTypeName = "Handler") : GeneratedHandlerAttribute`. A new
`ReducerHandlerEmitter` hangs off the existing `ActionFact` (no second scan) with its own tracking step,
`State.Emit.Reducer`. TWS0014 is driven by the same `ReducerFacts.Analyze` rule, linked into the analyzer, so the
generator and the analyzer cannot drift. The emitted handler qualifies every name with `global::`. It carries a
`[GeneratedCode]` marker, so the analyzer does not count it as hand-written. Its accessibility matches the Action's.
`Store` and `Reduce` resolve correctly: `Store` is an inherited member, and `Reduce` is called through the action set.

Risk is low. All the task's acceptance criteria hold:
- The pin bump is in the same commit as the feature.
- Release tracking is updated.
- The end-to-end test sends the action through `ISender<ClientPipeline>` and checks that state changed.
- The version is already 12.0.0-beta.17, and NuGet's latest is 12.0.0-beta.16, so no second bump is needed.

I found one nit and no bugs.

Gates re-run by the reviewer:
- `./bin/dev build`: exit 0, 0 errors.
- `dotnet test --no-build` on client-integration-tests (67 passed, 1 skipped), timewarp-state-analyzer-tests (77 passed)
  and timewarp-state-source-generator-tests (174 passed).
- The generated `...CounterState+DecrementCountActionSet.Handler_Reducer.g.cs` exists in the test app.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-state-source-generator/facts/reducer-facts.cs:121
- Description: Rule 5 checks only the Action's `DeclaredAccessibility`. It does not check the Action's effective
  accessibility. Consider a `public` Action inside a `private` (or `protected`) `*ActionSet`. The rule accepts it, and the
  generator emits a `public` Handler nested in the private set. Mediator.g.cs then fails with CS0122 instead of a TWS0014
  that says why. The design comment says the Handler is reachable by Mediator, and this case contradicts it. A
  hand-written handler fails the same way, so this is not a regression. It is a gap in TWS0014's "says what to fix"
  coverage for an unusual shape.
- Suggestion: Walk `actionSet` and its containing types, and fail when any of them is `private`, `protected` or `private
  protected`, with a reason like "'X' is private, so Mediator cannot reach the generated handler". Add one analyzer test.
- Status: open
