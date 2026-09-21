# Review framework — task 062

**Date:** 2026-09-21
**Host task:** kanban/in-progress/062-fix-timewarpstatecomponent-parameter-change-bugs/
**Diff scope:** branch `task/062-fix-timewarpstatecomponent-parameter-change-bugs` vs `origin/master` (implement commit `6706669e`; kanban results `51441d69`)
**Plan / brief:** Code review 2026-06-11 findings 8 and 9. `CheckParameterChanged` ran before `base.SetParametersAsync`, so `property.GetValue(this)` was the old value and `ParameterView` was incoming, but the locals were named the other way around. Directional overrides and trace logs were inverted. The leftover `ShouldRender` `throw new Exception("WTF")` was reachable when `HandleUnregisteredParameter` returned true without a `RenderReasonDetail`.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0c476-d4c8-7683-8f59-dfe71cf632f5` (2026-09-21)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state/components/timewarp-state-component.check-complex-parameter-changed.cs`
- `source/timewarp-state/components/timewarp-state-component.cs`
- `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs` (new)
- Surrounding call sites: `CheckPrimitiveParameterChanged` / `CheckCollectionParameterChanged` / `CheckComplexParameterChanged` / `HandleUnregisteredParameter`; sample override `tests/test-app/test-app-client/pages/should-render-test-page/ChildComponentWithComplexConstrained.razor`; existing harness `tests/timewarp-state-tests/timewarp-state-component/capture-render-caller-tests.cs`

## Requirements to check

- Locals match the documented `CheckComplexParameterChanged(parameterName, currentValue, incomingValue)` contract: `property.GetValue(this)` is current; `parameter.Value` is incoming
- Primitive and collection comparators share those locals (same un-swap)
- `throw new Exception("WTF")` is gone from `ShouldRender`
- `HandleUnregisteredParameter == true` sets a sensible `RenderReasonDetail` (setter is private; derived classes cannot set it)
- Test: override of `CheckComplexParameterChanged` receives old value as `currentValue`, new value as `incomingValue`
- Test: derived component overriding `HandleUnregisteredParameter` to return true re-renders instead of throwing
- Out of scope: changing `RenderReason` to `UntrackedParameter`; making `RenderReasonDetail` settable from derived classes
