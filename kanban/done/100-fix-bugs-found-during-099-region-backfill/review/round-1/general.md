# Round 1 — general
**Date:** 2026-10-09
**Scope reviewed:** origin/master...HEAD (47eeaeed)

## Summary
The four library fixes and the test-app fixes match the task brief. The new tests for the analyzer, StartHandler, InvalidCloneException, cacheable `IState<T>`, `EnumerableEqual` and weather `Days` would fail before the change. The analyzer exception is narrow and correct: concrete classes, wrong ordinals and constraints that name some other type all still report. The main problem is the `EmptyGuid` message. Its remedy ("use the default cloner") is wrong when the default cloner itself produced the empty Guid. The other findings are an over-broad release-note claim, an untested server throw endpoint, and a required `days` parameter.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state/features/pipeline/invalid-clone-exception.cs:48-53
- Description: The `EmptyGuid` message tells the user to "implement ICloneable.Clone so the clone is constructed, or use the default cloner, which leaves members marked [IgnoreDataMember] ... at their constructor values so Guid is regenerated." The default cloner is a common source of an empty Guid. `DeepCloner.CreateFactory` / `InvokeOrUninitialized` (source/timewarp-state/features/cloning/deep-cloner.cs:363-398) falls back to `RuntimeHelpers.GetUninitializedObject` when the chosen constructor throws (`TargetInvocationException`) or no usable constructor exists. That object has `Guid == Guid.Empty` (state.cs:40 initializer never ran). For a non-`ICloneable` state this happens on the default cloner path (state-transaction-behavior.cs:75-83), so the advice sends the user to the thing that already failed. The real fix in that case is to give the state a parameterless constructor, or a constructor that does not throw when it gets default arguments. Task item 2 asked for the message to state the real cause and the fix.
- Suggestion: Say that the default cloner falls back to an uninitialized instance when the state's constructor throws or none can be called, and tell the user to make sure the parameterless (or fewest-parameter) constructor runs with default arguments. Better: `StateTransactionBehavior` knows whether `ICloneable` was used, so pass that in and word the message for that path. Add a test with a non-`ICloneable` state whose parameterless constructor throws, and assert `Cause.EmptyGuid` and the wording.
- Status: open

### Issue 2 — Severity: suggestion
- File: documentation/release-notes/release12.0.0-beta.10.md:13
- Description: "A declaration that passed any other type no longer compiles" is not true. Take `sealed class A : TimeWarpCacheableState<B>` where `B : TimeWarpCacheableState<B>`. It satisfies the new constraint and compiles, and `A` becomes `State<B>` / `IState<B>` with `Hydrate` returning `B`. `StateInheritanceAnalyzer` does not catch it either. It only checks a class whose first base type is `State<T>` directly (state-inheritance-analyzer.cs:56-63), so classes that derive through the intermediate are never checked. The Design region at source/timewarp-state-plus/state/timewarp-cacheable-state.cs:9-10 ("a concrete class must still pass itself to State<T>") implies that protection exists. This hole existed before, but the new wording promises a guarantee the change doesn't deliver.
- Suggestion: Reword the note to say only types that satisfy `TimeWarpCacheableState<TState>` compile, and that the argument must be the derived state itself. Or extend the analyzer to walk the base chain to the `State<T>` it closes over and require `T == derived` for non-abstract classes, with a test for `A : TimeWarpCacheableState<B>`.
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/test-app/test-app-server/program.cs:72-80
- Description: The new server throw endpoint and the change that sends `action.Message` have no test that would fail before the fix. The only coverage is `tests/test-app-end-to-end-tests/throw-exception-page-tests.cs`, which asserts that `CounterState.Guid` is unchanged. That passed before too, when the unmapped route returned 404. The requirement says each fix gets a test that fails before and passes after.
- Suggestion: Add a client-integration test (the WebApplicationFactory host already exists in infrastructure/testing-convention.cs). Send `ThrowServerSideExceptionActionSet.Action` and assert the failure is the server's 500, not 404. Or GET `ThrowServerSideExceptionRequest.GetRoute()` and assert 500 plus the message.
- Status: open

### Issue 4 — Severity: nit
- File: tests/test-app/test-app-server/program.cs:69
- Description: `int days` is now a required query parameter. A GET to plain `api/weather` (the old contract route, or a manual browser or curl check) now returns 400 instead of forecasts.
- Suggestion: Use `int days = 5` (or `int? days`) so the old route still works, unless strict binding is intended.
- Status: open
