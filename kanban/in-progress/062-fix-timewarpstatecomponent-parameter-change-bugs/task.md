# Fix TimeWarpStateComponent parameter-change bugs

## Description

Code review 2026-06-11, findings 8 and 9 (`code-review-2026-06-11.md`).

Two related bugs in the base component's parameter handling:

1. **Swapped current/incoming values** — `source/timewarp-state/components/timewarp-state-component.check-complex-parameter-changed.cs:92–93`:
   ```csharp
   object? newValue = property.GetValue(this);   // actually the OLD value (runs before base.SetParametersAsync)
   object? currentValue = parameter.Value;        // actually the INCOMING value
   ```
   The virtual `CheckComplexParameterChanged(parameterName, currentValue, incomingValue)` (documented contract at lines 174–175) receives its arguments reversed. Harmless for the symmetric default `ReferenceEquals`, but any override with directional logic is inverted, and the type names in trace logs are swapped.

2. **Reachable leftover debug throw** — `source/timewarp-state/components/timewarp-state-component.cs:142–143`: `if (ParameterTriggered && RenderReasonDetail is null) throw new Exception("WTF")` (marked `// TODO: Remove`). Reachable: `CheckParameterChanged` returns `HandleUnregisteredParameter(parameter)` (line 89) without setting `RenderReasonDetail`, and `RenderReasonDetail` has a private setter — so a derived class using the documented virtual `HandleUnregisteredParameter` extension point (returning true) cannot avoid the throw.

## Checklist

- [x] Swap the two assignments (or rename the locals) so current/incoming match the documented contract
- [x] Remove the `"WTF"` throw; ensure `HandleUnregisteredParameter == true` sets a sensible `RenderReasonDetail`
- [x] Test: an override of `CheckComplexParameterChanged` receives old value as `currentValue`, new value as `incomingValue`
- [x] Test: derived component overriding `HandleUnregisteredParameter` to return true re-renders instead of throwing

## Session

- Created: code review 2026-06-11
- Implementer: grok session 01a0c46e-665c-7da0-8ea8-efc060a17c69 (2026-09-21)

## Results

`CheckParameterChanged` now passes the component's current value as `currentValue` and `ParameterView`'s value as `incomingValue`, matching the documented `CheckComplexParameterChanged` contract (finding 8). Primitive and collection comparators share those locals, so they were un-swapped in the same change. The leftover `throw new Exception("WTF")` is gone (finding 9). When `HandleUnregisteredParameter` returns true, `RenderReasonDetail` is set to `Parameter '{name}' changed: Unregistered parameter` so a derived override can re-render instead of crashing.

**What landed**

- Locals renamed: `property.GetValue(this)` is current; `parameter.Value` is incoming.
- `HandleUnregisteredParameter == true` calls `SetRenderReasonForParameterChange` before `ShouldRender`.
- Debug throw removed from `ShouldRender`.

**Files**

- `source/timewarp-state/components/timewarp-state-component.check-complex-parameter-changed.cs`
- `source/timewarp-state/components/timewarp-state-component.cs`
- `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs` (new)

**Decisions**

- Set `RenderReasonDetail` in `CheckParameterChanged` when the unregistered hook returns true. Derived classes cannot set it (private setter).
- Did not change `ShouldRender` to use `RenderReasonCategory.UntrackedParameter`; that enum member is unused and out of scope. `ShouldRender` still classifies this path as `ParameterChanged`.

**Tests**

`dotnet run --file ./scripts/test.cs` exit 0:

- analyzer 19 passed
- source generator 4 passed
- state 33 passed, 1 skipped (includes two `ParameterChangeTests.Should_` cases)
- plus 25 passed, 1 skipped
- client integration 42 passed, 1 skipped
- architecture 7 passed, 1 skipped

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests
```

**Expect**

- Exit 0.
- `ParameterChangeTests.Should_.CheckComplexParameterChanged_Receives_Current_Then_Incoming` passes: after a first `SetParametersAsync` that seeds `Model`, a second call with a different instance reports the seed as `currentValue` and the new instance as `incomingValue`.
- `ParameterChangeTests.Should_.HandleUnregisteredParameter_True_Rerenders_Without_Throwing` passes: a derived component that returns true from `HandleUnregisteredParameter` does not throw; `ShouldRender` is true; `RenderReasonDetail` is `Parameter 'Unknown' changed: Unregistered parameter`.
- `source/timewarp-state/components/timewarp-state-component.cs` does not contain `throw new Exception("WTF")`.

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; analyzer 19, generator 4, state 33+1 skipped, plus 25+1 skipped,
# client integration 42+1 skipped, architecture 7+1 skipped
```

**Not in scope:** changing `RenderReason` to `UntrackedParameter`; making `RenderReasonDetail` settable from derived classes.
