# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** same as framework (856ff215 vs merge-base 47607cd2; product paths only)

## Summary

The change is documentation, one sample migration and a new analyzer test file. No generator, analyzer or attribute
code changed, which matches the task. It fits both the original task and the implementer restatement, and I found
no misread. Risk is low. The new tests run through the existing analyzer test harness, and the sample builds with
the generated handlers in place.

## Verification

- `dotnet test tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj`: Passed 90, Failed 0 (77
  before this task plus 13 new `ReducerShapeAnalyzers_` tests).
- `dotnet build` of `samples/05-persistence/wasm/sample-05-wasm/sample-05-wasm.csproj`: 0 errors. A
  `--no-incremental` rebuild shows no warning in any file under `samples/05-persistence`. The generated folder has
  `DisplayPreferencesState+SetAccentActionSet.Handler_Reducer.g.cs` and `+ToggleCompactActionSet.Handler_Reducer.g.cs`.
- Doc claims checked against source:
  - The generated `Handler` sample in `actions.md` matches `reducer-handler-emitter.cs`: `IStore` primary ctor,
    `StateActionHandler<Action>`, a call to `ActionSet.Reduce(Store.GetState<TState>(), action)`, `return default`,
    `[GeneratedCode("TimeWarp.State.SourceGenerator", "1.0.0")]`, and the Action's accessibility.
  - The dispatch method signature `(…, CancellationToken? externalCancellationToken = null)` matches the generated
    `_Method.g.cs` files.
  - `ReducerAttribute : GeneratedHandlerAttribute` (`source/timewarp-state/attributes/reducer-attribute.cs:41`).
  - The TWS0014 rule list in `actions.md` matches `analyzers.md`.
  - The sample readme says persistence "saves after each one". This holds:
    `PersistentStatePostProcessor<TRequest,TResponse>` keys on the action's enclosing state type, not on the
    handler, so the short shape is covered too.
- Doc links: the `xref:TimeWarpState:Actions.md` uid matches the new topic's front matter, and `toc.yml` lists it.
  `../../samples/05-persistence/readme.md` is in the docfx content set (`samples/**/*.md`). Both follow
  `documentation/contributing/overview.md` link rules.
- Requirement coverage:
  - Side-by-side shapes, what each emits, and when to hand-write a Handler: done.
  - `readme.md`, `claude.md` and `summary.md`: updated.
  - Tests for TWS0001, TWS0004–0008 and TWS0010/0011, plus `[Reducer]` together with `[CatalogAction]`: present, as
    both should-not-report and still-reports cases.
  - Sample 05 has a short-shape state (`DisplayPreferencesState`) and long-shape actions (`DraftNoteState`).
  - The release-note Authoring section is present. The version is unchanged.
  - `AnalyzerReleases.Unshipped.md` is unchanged, correctly, because no analyzer was changed.

## Issues

None.

## Observations (not findings)

- The `readme.md` "Writing an action" snippet puts `await CounterState.IncrementCount(amount: 5);` after a type
  declaration. That order would not compile as a single file. The snippet is illustrative, the trailing comment says
  so, and other readme snippets mix declarations and calls the same way, so I did not raise it.
