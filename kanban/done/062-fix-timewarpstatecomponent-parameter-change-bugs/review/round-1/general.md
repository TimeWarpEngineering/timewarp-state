# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** implement commit `6706669e` vs `origin/master` — findings 8 and 9 (`timewarp-state-component.check-complex-parameter-changed.cs`, `timewarp-state-component.cs`, `parameter-change-tests.cs`; surrounding `ChildComponentWithComplexConstrained.razor`, `capture-render-caller-tests.cs`)

## Summary

The change renames `CheckParameterChanged` locals so `property.GetValue(this)` is `currentValue` and `parameter.Value` is `incomingValue`, then threads those into the primitive, collection, and complex comparators in documented order. It also removes the leftover `ShouldRender` `"WTF"` throw and sets `RenderReasonDetail` when `HandleUnregisteredParameter` returns true (private setter, so the base must set it). Risk is low: a small, targeted correctness fix with two focused unit tests that both passed under `dotnet fixie timewarp-state-tests`.

## Issues

