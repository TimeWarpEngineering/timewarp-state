---
uid: TimeWarpState:RenderControl.md
title: Render control
---

# Control when a component renders

`TimeWarpStateComponent` replaces `ShouldRender`. Each pass that returns true has a `RenderReason`.

| Category | When `ShouldRender` assigns it |
| --- | --- |
| `Event` | The pass is not a parameter set, a subscription check, or `ReRender` |
| `ParameterChanged` | `SetParametersAsync` saw a changed parameter |
| `Subscription` | A subscribed state changed and `ShouldReRender` returned true |
| `Forced` | `ReRender()` requested the pass |

`RenderReasonDetail` names the parameter when the category is `ParameterChanged`. An unregistered name uses the detail `Parameter 'Unknown' changed: Unregistered parameter` when `HandleUnregisteredParameter` returns true.

## Subscribe where you read

`GetState<T>()` subscribes the calling component and returns the state. `GetState<T>(placeSubscription: false)` reads without subscribing. Put the subscribing call in the component whose markup reads that state.

With no render trigger, every action on that state calls `ReRender`. Register one property when the markup depends on part of the state:

```csharp
protected override void OnInitialized()
{
  base.OnInitialized();
  RegisterRenderTrigger<ActivityState>(state => state.Count);
}
```

A `Beat` change then returns false from `ShouldReRender`, and this component does not render. A `Count` change still renders, and `ShouldRender` reports `Subscription`. The store still clones the state for the action. The trigger skips the component render, not the clone.

## Parameters

Parameter comparison runs when a derived type overrides `CheckPrimitiveParameterChanged`, `CheckCollectionParameterChanged`, `CheckComplexParameterChanged`, or `HandleUnregisteredParameter`. Without an override, a parameter change that reaches the component is `Event` and is not named.

Override the primitive check and call the base method when the category should be `ParameterChanged` and the detail should name the parameter. Override `CheckComplexParameterChanged` when a new instance can carry the same values. The arguments are the current value, then the incoming value. Return false to skip the render.

`HandleUnregisteredParameter` is the hook for a `ParameterView` entry that is not a `[Parameter]` or `[CascadingParameter]`. Return true to count that name as a change. The base class sets `RenderReasonDetail`. Blazor still assigns the view and throws if the name is not a property. The base hook returns false.

## Render mode

`RendererInfo.Name` and `RendererInfo.IsInteractive` are the host mode. `IsPreRendering` is `!RendererInfo.IsInteractive`. `AssignedRenderMode` is the mode Blazor assigned to the component. On the WebAssembly sample that type name is `InteractiveWebAssemblyRenderMode`.

## Caller names

`TimeWarpStateOptions.CaptureRenderCaller` defaults to false. Set it on a diagnostic host that shows `ShouldRenderWasCalledBy`, `SetParametersAsyncWasCalledBy`, or `StateHasChangedWasCalledBy`. Those strings are `Class.Method`. When the flag is true, each of those three methods allocates a `StackTrace`.

## Sample

[06-Render control](../../samples/06-render-control/readme.md) is a WebAssembly page: label children, a filter compared by reference and one compared by text, a card that renders on every `ActivityState` action, and a card that renders only when `Count` changes.
