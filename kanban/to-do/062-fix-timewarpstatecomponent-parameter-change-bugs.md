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

- [ ] Swap the two assignments (or rename the locals) so current/incoming match the documented contract
- [ ] Remove the `"WTF"` throw; ensure `HandleUnregisteredParameter == true` sets a sensible `RenderReasonDetail`
- [ ] Test: an override of `CheckComplexParameterChanged` receives old value as `currentValue`, new value as `incomingValue`
- [ ] Test: derived component overriding `HandleUnregisteredParameter` to return true re-renders instead of throwing
