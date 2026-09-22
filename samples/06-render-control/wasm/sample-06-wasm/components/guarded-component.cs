#region Purpose
// Opts a component into parameter diffing so an unchanged parent render can be skipped.
#endregion

#region Design
// Parameter comparison runs only when a derived type overrides CheckPrimitiveParameterChanged,
// CheckCollectionParameterChanged, CheckComplexParameterChanged, or HandleUnregisteredParameter.
// Without that override, SetParametersAsync leaves its flag clear and ShouldRender treats the
// parent render as Event, so the child paints every time the parent does.
// This type overrides the primitive check and keeps the base equality test.
#endregion

namespace Sample06Wasm.Components;

/// <summary>
/// Skips a render when primitive parameters are unchanged.
/// </summary>
public class GuardedComponent : TracingComponent
{
  protected override bool CheckPrimitiveParameterChanged(object? currentValue, object? newValue)
  {
    return base.CheckPrimitiveParameterChanged(currentValue, newValue);
  }
}
