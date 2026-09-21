# Fix JsonRequestHandler DotNetObjectReference leak (remainder)

## Description

Code review 2026-06-11, finding 10. **Partly done:** `TimeWarpJavaScriptInterop.razor` already has `if (firstRender)` (080-003). Remaining:

`JsonRequestHandler.InitAsync` still `DotNetObjectReference.Create(this)` on every call, no `IsInitialized` guard, no stored reference, no `Dispose`. Contrast `ReduxDevToolsInterop.InitAsync`.

## Requirements

- `IsInitialized` guard in `InitAsync` (no-op if already initialized)
- Hold `DotNetObjectReference<JsonRequestHandler>` in a field; dispose in `Dispose`/`DisposeAsync`; swallow `JSDisconnectedException`
- Keep the existing firstRender guard
- Test or documented proof: N renders → one Create

## Out of scope

- Redux DevTools interop (058)
- Changing the JS `InitializeJavaScriptInterop` contract except as needed for dispose

## Checklist

- [x] firstRender in TimeWarpJavaScriptInterop (080-003)
- [ ] IsInitialized in InitAsync
- [ ] Store + dispose the reference
- [ ] One reference across N renders

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit — firstRender already on master; remainder only. Sequence after this: 062 → 061 → 060 → 066 → 067 (one at a time).
