# Task 108: H4: Changed collection parameters can fail to re-render (complex parameter comparison)

## Description

`TimeWarpStateComponent` compares collection parameters by count only
(`check-complex-parameter-changed.cs`), so a same-length replacement list can be judged unchanged and the
child renders stale items; non-primitive value types are boxed and always count as changed. Not in Fable's
ordered list; schedule after 107.

Filed 2026-10-10 at Steven's request (relayed by Amina) from Claude Fable's full codebase review,
task 102 (`kanban/done/102-full-codebase-review-by-claude-fable-review-only/`, PR #629, `00ade373`).
Not launched.

## Finding H4 (Fable review)

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 48-53 (PR #629, merged as `00ade373`):

> ### H4. `TimeWarpStateComponent` collection parameter check suppresses renders when any check hook is overridden
>
> - `source/timewarp-state-blazor/components/timewarp-state-component.check-complex-parameter-changed.cs:189` compares collections by `Count()` only. `timewarp-state-component.cs:123` onward returns `false` from `ShouldRender` when `SetParametersAsync` ran, no parameter was judged changed, and no subscription or `ReRender` flag is set.
> - Scenario: a component that overrides `HandleUnregisteredParameter` or any `Check*` method (which `documentation/topics/render-control.md` recommends) receives a `List<Item>` parameter. The parent replaces the list with a different list of the same length. `CheckForOverriddenMethods` is true, the collection check says "unchanged", `ParameterTriggered` stays false, `ShouldRender` returns false, and the child shows stale items. The e2e `should-render-test-page` has a `ChildComponentWithCollection`, but the test app overrides are on a base component, so only the overriding case is exercised.
> - Related: `:132` treats only `IsPrimitive` and `string` as primitives, so `decimal`, `DateTime`, `Guid`, `enum` and user structs fall through to `:222`, where boxing makes `ReferenceEquals` always false. That direction is safe (over-render) but makes `RenderReasonDetail` claim a change that did not happen, which defeats the diagnostic purpose.
> - Suggestion: for collections, fall back to element-wise `SequenceEqual` on materialized snapshots, or treat "same count" as "unknown" and return true; never enumerate an `IQueryable`. Route non-primitive value types through `Equals`. Add a unit test in `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs` for same-count-different-items.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Requirements

- Collections: element-wise `SequenceEqual` on materialized snapshots, or treat same count as unknown (render);
  never enumerate an `IQueryable`.
- Non-primitive value types (`decimal`, `DateTime`, `Guid`, enums, structs) compared with `Equals`.
- Unit test in `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs` for
  same-count-different-items.

## Checklist

- [x] Fix collection comparison (`:189`)
- [x] Route non-primitive value types through `Equals` (`:132`, `:222`)
- [x] Tests: same-count-different-items re-renders; equal value-type parameter does not report a change
- [ ] Code review

## Acceptance criteria

- With a `Check*`/`HandleUnregisteredParameter` override present, replacing a list with a different list of the
  same length re-renders the child.
- `RenderReasonDetail` no longer reports a change for an equal `DateTime`/`Guid`/`decimal` parameter.
- Tests green; `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)
- Implementer: Grok session 01a126fb-2629-7b71-9d9e-f5aa777d2134 (2026-10-11)

## Notes

- Source: 102 review-findings.md finding H4. Related batch: 109 (Medium findings).

## Results

Collection parameters are compared element by element on a snapshot. Non-primitive value types use `Equals`. A same-length replacement list re-renders when any check hook is overridden, and an equal `DateTime`, `Guid`, or `decimal` does not write a new `RenderReasonDetail`.

### Files

- `source/timewarp-state-blazor/components/timewarp-state-component.check-complex-parameter-changed.cs`
- `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs`
- `documentation/topics/render-control.md`

### Decisions

- Collections use `SequenceEqual` on materialized snapshots. Order is significant. Equal contents do not count as a change, so the diagnostic does not claim one.
- `IQueryable` is never enumerated. A different instance counts as changed.
- `decimal`, `DateTime`, `Guid`, enums, structs, and other value types, including `Nullable<T>`, go through `CheckPrimitiveParameterChanged` (`Equals`). Other reference types still compare by reference.

### Tests

`dotnet fixie timewarp-state-tests`: 110 passed, 1 skipped. New cases passed: `SameCount_DifferentItems_Rerenders`, `SameItems_DoNotReportAChange`, `Equal_ValueType_Parameters_DoNotReportAChange`, `Query_IsNotEnumerated_And_ADifferentInstance_Rerenders`.

`ganda repo audit` exited 0. The kebab-path advisory is the pre-existing Blazor `lib.module` filenames.

Code review stays open for the host review node. This walk did not run `tw-implementation-review`.

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests
```

**Expect**

Exit 0. The log runs these tests and they pass:

- `ParameterChangeTests.Should_.SameCount_DifferentItems_Rerenders`
- `ParameterChangeTests.Should_.SameItems_DoNotReportAChange`
- `ParameterChangeTests.Should_.Equal_ValueType_Parameters_DoNotReportAChange`
- `ParameterChangeTests.Should_.Query_IsNotEnumerated_And_ADifferentInstance_Rerenders`

**Automated gate**

```bash
ganda repo audit
```

Expect exit 0.

**Not in scope:** the should-render browser page. `ChildComponentWithCollection` does not override a check hook, so that page still takes the event path. The unit tests cover the override case.
