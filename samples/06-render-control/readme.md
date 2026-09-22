# Render control sample

Blazor WebAssembly page for `TimeWarpStateComponent`: which pass paints, and two ways to skip a pass.

| Piece | What it shows |
| --- | --- |
| Host strip | `RendererInfo.Name` (`WebAssembly`), `RendererInfo.IsInteractive` (`True`), `IsPreRendering` (`False`), `AssignedRenderMode` (`InteractiveWebAssemblyRenderMode`) |
| Open child | A changed string paints as `Event` and does not name the parameter |
| Guarded child | The same string change is `ParameterChanged` and names `Label` |
| Reference filter | No complex check. A page render paints it, including a new `FilterModel` with the same text |
| Text filter | `CheckComplexParameterChanged` compares `Text`. Same text skips. A new text is `ParameterChanged` |
| Every change | `GetState<ActivityState>()` with no trigger. `Count` and `Beat` both paint it |
| Count only | `RegisterRenderTrigger<ActivityState>(state => state.Count)`. `Beat` does not paint it |

## Run

Pack the in-tree package first when `artifacts/packages` does not already contain this repo's `TimeWarp.State` version:

```bash
./bin/dev pack
dotnet run --project samples/06-render-control/wasm/sample-06-wasm/sample-06-wasm.csproj --launch-profile http
```

NuGet keeps the first copy of `12.0.0-beta.4` it restores. A later pack of that same version does not replace `~/.nuget/packages/timewarp.state/12.0.0-beta.4`. Delete that version folder and restore again so the sample binds the packed assembly.

Open `http://localhost:5296`.

This sample does not reference TimeWarp.State.Plus.

## Render mode

`IsPreRendering` is `!RendererInfo.IsInteractive`. On this host the name is `WebAssembly`, interactive is true, and prerendering is false. `AssignedRenderMode` is `InteractiveWebAssemblyRenderMode`. A Server or Auto host reads the same properties. This sample is one host.

## What to try

The render figure on each card includes the pass on screen. `RenderCount` increments in `OnAfterRender`, after the markup is produced. The first pass does not call `ShouldRender`, so each trace starts empty. A detail on that first paint is the initial parameter assignment (null or empty to the first value). Later passes append a line. The list keeps the last 40 lines. The figure is the full count.

The caller column is `Class.Method` for whoever called `ShouldRender`. On this page that caller is `ComponentBase.StateHasChanged`. The sample reads the frame in its own override, and only when `CaptureRenderCaller` is true, because that flag's frame would otherwise name the override.

### Parameters

1. **Tick local field.** This page records `Event`. The label children do not paint. The reference filter does, because it has no complex check. The text filter does not.
2. **Swap label.** Both label children paint. The open child records `Event` and leaves the detail blank. The guarded child records `ParameterChanged` and the detail `Parameter 'Label' changed`.
3. **New wrapper, same text.** The reference filter paints (`Event`). The text filter does not. Its `CheckComplexParameterChanged` compares `FilterModel.Text`. The arguments are the current value, then the incoming value.
4. **Change filter text.** Both filter children paint. The text filter records `ParameterChanged` and names `Filter`. The reference filter records `Event`.
5. **Schedule forced render.** The click yields, then calls `ReRender()`. This page's trace keeps a `Forced` line between `Event` lines from the handler.
6. **Admit unknown parameter** (on the guarded card). The button builds a `ParameterView` that includes `Unknown`. `HandleUnregisteredParameter` returns true, so the trace records `ParameterChanged` and the detail `Parameter 'Unknown' changed: Unregistered parameter`. Blazor then throws because the card has no such property. The card catches that and shows it as the expected refusal. The base hook returns false.

Parameter comparison runs when a derived type overrides `CheckPrimitiveParameterChanged`, `CheckCollectionParameterChanged`, `CheckComplexParameterChanged`, or `HandleUnregisteredParameter`. `GuardedComponent` overrides the primitive check and calls the base equality test, which is what names `Label`. `TextFilterChild` overrides the complex check. Without an override, `SetParametersAsync` does not mark the pass, and a delivered parameter change is `Event`.

### Subscriptions

`ActivityState` has `Count` and `Beat`. Both cards subscribe by calling `GetState<ActivityState>()` while they render. The page sends with `GetState<ActivityState>(placeSubscription: false)`, so the page is not a subscriber. Those cards inherit `GuardedComponent`, so a render of the page does not paint them.

1. **Tick once** or **Tick 25 times.** The every-change card records `Subscription` once per action. The burst waits a millisecond between actions so Blazor does not fold those requests into one paint. The render figure climbs by 1, then by 25. `Beat` shows the new total. The count-only card registers `Count`. A `Beat` change makes `ShouldReRender` return false, so `ReRender` is not called. That card's `Beat` and render figure stay put.
2. **Increment count.** Both cards render once. The count-only card's `Beat` catches up, because the render reads the current state. The category is `Subscription`. A trigger that allows the render does not surface as its own category.

Each action still clones `ActivityState` (`StateTransactionBehavior`). The trigger skips the component render, not the clone.

The tick buttons ignore a second click while a send is in flight. A canceled send (leaving the page) is not shown as an error. Any other exception is shown above the cards.

## Caller column

This host sets `TimeWarpStateOptions.CaptureRenderCaller`. Leave the flag false on a production host. When it is true, `ShouldRender`, `SetParametersAsync`, and `StateHasChanged` each allocate a `StackTrace`. This page allocates one more frame inside its `ShouldRender` override so the column is not the override itself.

`UseReduxDevTools()` is on the host so `CommitHandler` can be constructed when Development validates DI. The sample does not render `<ReduxDevTools />`, so the extension is not initialized and actions are not forwarded.

## Practices

- Subscribe in the component that reads the state. `GetState` there is the subscription.
- Register the property the markup depends on. A card that shows `Count` should not render for `Beat`.
- Override `CheckComplexParameterChanged` when a new instance can carry the same values. Compare the fields you render. The arguments are current, then incoming.
- Override a primitive check when you want `ParameterChanged` and the parameter name. Without it, a delivered change is `Event`.
- Call `ReRender()` for a pass that is not the event handler's own pass. The category is `Forced`. Yield first if the click would replace that line with `Event`.
- Leave `CaptureRenderCaller` false except on a diagnostic host.
- Return true from `HandleUnregisteredParameter` only when that extra name should count as a change. Blazor still assigns the `ParameterView` and throws if the name is not a property.
- The enum includes `StateHasChanged`. A direct call made while the other flags are clear is still classified as `Event`, because that check runs first. This page does not bind a button to that category.

## Host checklist

- Inherit `TimeWarpStateComponent`.
- `[assembly: MediatorScope(typeof(ClientPipeline))]`.
- `AddGeneratedMediator<ClientPipeline>()`.
- `AddTimeWarpState`. Set `CaptureRenderCaller` only on a diagnostic host.
- `options.UseReduxDevTools()` when the generated mediator links `CommitHandler` and Development validates the container. Skip `<ReduxDevTools />` when the extension is not part of the lesson.
