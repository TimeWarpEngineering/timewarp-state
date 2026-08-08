# Fix JsonRequestHandler DotNetObjectReference leak

## Description

Code review 2026-06-11, finding 10 / table row 9 (`code-review-2026-06-11.md`).

`source/timewarp-state/features/javascript-interop/json-request-handler.cs:78–83`: `InitAsync` has no initialization guard (contrast `ReduxDevToolsInterop.InitAsync:58`, which has one) and creates `DotNetObjectReference.Create(this)` on every call with no disposal. `TimeWarpJavaScriptInterop.razor:8–11` calls it from `OnAfterRenderAsync` with **no `firstRender` check**, and the JS side (`InitializeJavaScriptInterop`) just overwrites `timeWarpState.jsonRequestHandler`, orphaning the prior reference.

Net effect: one leaked, still-invokable `DotNetObjectReference` per render of any page containing `TimeWarpJavaScriptInterop`, pinned for the circuit lifetime.

## Fix

- `if (firstRender)` guard in `TimeWarpJavaScriptInterop.OnAfterRenderAsync`
- `IsInitialized` guard inside `InitAsync` (defense in depth, matching the ReduxDevToolsInterop pattern)
- Hold the created `DotNetObjectReference` in a field and dispose it in `Dispose`/`DisposeAsync` (handle `JSDisconnectedException`)

## Checklist

- [ ] firstRender guard in the component
- [ ] IsInitialized guard in InitAsync
- [ ] Store + dispose the DotNetObjectReference
- [ ] Verify only one reference is created across N renders

## Notes

This JS interop channel is general-purpose (not DevTools-specific), so it survives the planned ReduxDevTools removal (task 058) and is worth fixing independently.
